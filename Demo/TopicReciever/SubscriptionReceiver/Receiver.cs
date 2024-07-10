using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Identity;

ServiceBusClient client;

ServiceBusProcessor processor;

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body} from subscription.");

    // complete the message. messages is deleted from the subscription. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

// the client that owns the connection and can be used to create senders and receivers
// The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.
client = new ServiceBusClient(
    "ServiceBusDemoNamespace.servicebus.windows.net",
    new DefaultAzureCredential());

// create a processor that we can use to process the messages
processor = client.CreateProcessor("mytopic", "S1", new ServiceBusProcessorOptions());

try
{
    // add handler to process messages
    processor.ProcessMessageAsync += MessageHandler;

    // add handler to process any errors
    processor.ProcessErrorAsync += ErrorHandler;

    // start processing 
    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    // stop processing 
    Console.WriteLine("\nStopping the receiver...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    
    await processor.DisposeAsync();
    await client.DisposeAsync();
}