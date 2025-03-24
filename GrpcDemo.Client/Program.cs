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
    
    var response = client.UnaryDemo(request, deadline: DateTime.Now.AddSeconds(10));
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
    var request = new Request() { Content = "Client" };
    using var response = client.ServerStreamingDemo(request);

    await foreach (var responseItem in response.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(responseItem.Message);
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