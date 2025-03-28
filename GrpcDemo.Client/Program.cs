﻿// See https://aka.ms/new-console-template for more information

using Auth;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Reflection.V1Alpha;
using GrpcDemo.Client.Interceptors;
using GrpcDemo.Protos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServerReflectionClient = Grpc.Reflection.V1Alpha.ServerReflection.ServerReflectionClient;

//var clientOptions = new GrpcChannelOptions();
//using var channel = GrpcChannel.ForAddress("https://localhost:7222", clientOptions);
//var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);

var services = new ServiceCollection();
services.AddTransient<ClientRequestsLogger>();
services.AddGrpcClient<FirstServiceDefinition.FirstServiceDefinitionClient>(options =>
    {
        options.Address = new Uri("https://localhost:7222");
    }).ConfigureChannel(channelOptions => channelOptions.ServiceConfig = new ServiceConfig
    {
        MethodConfigs =
        {
            // Retry policy
            new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                },
                /*HedgingPolicy = new HedgingPolicy
                {
                    MaxAttempts = 5,
                    NonFatalStatusCodes = { StatusCode.Unavailable },
                    HedgingDelay = TimeSpan.FromMicroseconds(500)
                }*/
            }
        }
    })
    .AddInterceptor<ClientRequestsLogger>()
    .AddCallCredentials((_, metadata) =>
    {
        var token = JwtHelper.GenerateJwtToken("TestUser");
        if (!string.IsNullOrEmpty(token))
        {
            metadata.Add("Authorization", $"Bearer {token}");
        }
        return Task.CompletedTask;
    })
    ;

services.AddLogging(configure => configure.AddConsole());

var serviceProvider = services.BuildServiceProvider();

var client = serviceProvider.GetRequiredService<FirstServiceDefinition.FirstServiceDefinitionClient>();

await DiscoverAndDisplayGrpcServices();
await HealthCheck();
UnaryTest(client);
await ClientStreamingTest(client);
await ServerStreamingTest(client);
await DuplexStreamingTest(client);

Console.ReadLine();
return;

void UnaryTest(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var request = new Request() { Content = "Client" };
    var headers = new Metadata
    {
        { "Culture-Name", Thread.CurrentThread.CurrentCulture.Name },
        //{ "grpc-accept-encoding", "gzip"}
    };

    var response = client.UnaryDemo(request, deadline: DateTime.UtcNow.AddSeconds(30), headers: headers);
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

async Task HealthCheck()
{
    using var channel = GrpcChannel.ForAddress("https://localhost:7222");
    var healthClient = new Health.HealthClient(channel);
    var healthResult = await healthClient.CheckAsync(new HealthCheckRequest());
    
    Console.WriteLine($"Health Status: {healthResult.Status}");
}

async Task DiscoverAndDisplayGrpcServices()
{
    using var channel = GrpcChannel.ForAddress("https://localhost:7222");
    var serverReflectionClient = new ServerReflectionClient(channel);
    ServerReflectionRequest request = new ServerReflectionRequest
    {
        ListServices = ""
    };
    using var call = serverReflectionClient.ServerReflectionInfo();
    await call.RequestStream.WriteAsync(request);
    await call.RequestStream.CompleteAsync();
    
    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        foreach (var item in response.ListServicesResponse.Service)
        {
            Console.WriteLine(item.Name);
        }
    }
}
