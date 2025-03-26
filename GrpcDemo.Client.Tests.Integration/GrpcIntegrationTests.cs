using GrpcDemo.Protos;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GrpcDemo.Client.Tests.Integration;
public class GrpcIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GrpcIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void MyGrpcMethod_ShouldReturnExpectedResult()
    {
        // Create the gRPC client using the extension method
        var client = _factory.CreateGrpcClient<FirstServiceDefinition.FirstServiceDefinitionClient>();

        // Prepare the request
        var request = new Request() { Content = "Test" };

        // Call the gRPC method
        var response = client.UnaryDemo(request);

        // Assert the response
        Assert.Equal("Hello Test", response.Message);
    }
}