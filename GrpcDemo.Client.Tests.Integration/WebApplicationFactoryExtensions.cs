using System;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GrpcDemo.Client.Tests.Integration;

public static class WebApplicationFactoryExtensions
{
    public static TClient CreateGrpcClient<TClient>(this WebApplicationFactory<Program> factory) where TClient : class
    {
        // Create an HttpClient from the test factory
        var httpClient = factory.CreateClient();

        // Create a gRPC channel using the HttpClient
        var channel = GrpcChannel.ForAddress(httpClient.BaseAddress ?? new Uri("http://localhost"), new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        // Use the channel to create a gRPC client instance
        return Activator.CreateInstance(typeof(TClient), channel) as TClient
               ?? throw new InvalidOperationException($"Could not create an instance of {typeof(TClient).Name}");
    }
}
