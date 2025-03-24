// See https://aka.ms/new-console-template for more information

using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Protos;

var clientOptions = new GrpcChannelOptions();

using var channel = GrpcChannel.ForAddress("https://localhost:7222", clientOptions);

var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

UnaryTest(client);
await ClientStreamingTest(client);
await ServerStreamingTest(client);
await DuplexStreamingTest(client);

Console.ReadLine();

void UnaryTest(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request() { Content = "Client" };
    var headers = new Metadata
    {
        { "Content-Type", "application/grpc" },
        { "Culture-Name", Thread.CurrentThread.CurrentCulture.Name }
    };

    var response = client.UnaryDemo(request, deadline: DateTime.UtcNow.AddSeconds(10), headers: headers);
    Console.WriteLine(response.Message);
}

async Task ClientStreamingTest(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var clientStream = client.ClientStreamingDemo();
    for (var i = 0; i < 10; i++)
    {
        await clientStream.RequestStream.WriteAsync(new Request() { Content = i.ToString() });
    }
    await clientStream.RequestStream.CompleteAsync();
    var response = await clientStream;
    Console.WriteLine(response.Message);
}

async Task ServerStreamingTest(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var cancellationTokenSource = new CancellationTokenSource();
    var request = new Request() { Content = "Client" };
    using var response = client.ServerStreamingDemo(request);

    var random = new Random();
    try
    {
        await foreach (var responseItem in response.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
        {
            if (responseItem.Message.Contains(random.Next(1, 101).ToString()))
            {
                cancellationTokenSource.Cancel();
            }
            Console.WriteLine(responseItem.Message);
        }

        var trailers = response.GetTrailers();
        Console.WriteLine("Demo trailer value:" + trailers.GetValue("trailer-key"));
    }
    catch (RpcException e) when (e.StatusCode == StatusCode.Cancelled)
    {
        Console.WriteLine(e);
    }
}

async Task DuplexStreamingTest(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.DuplexStreamingDemo();
    for (var i = 0; i < 10; i++)
    {
        await call.RequestStream.WriteAsync(new Request() { Content = i.ToString() });
    }

    await foreach (var responseItem in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(responseItem.Message);
    }
    
    await call.RequestStream.CompleteAsync();
}