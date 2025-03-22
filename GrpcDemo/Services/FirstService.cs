using System.Text;
using Grpc.Core;
using GrpcDemo.Protos;
namespace GrpcDemo.Services;

public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
{
    public override Task<Response> Unary(Request request, ServerCallContext context)
    {
        var response = new Response
        {
            Message = "Hello " + request.Content
        };
        
        return Task.FromResult(response);
    }

    public override async Task<Response> ClientStreaming(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
    {
        var allContent = new StringBuilder();
        while (await requestStream.MoveNext())
        {
            var requestPayload = requestStream.Current;
            Console.WriteLine(requestPayload.Content);
            allContent.AppendLine(requestPayload.Content);
        }

   
        
        var response = new Response { Message = allContent.ToString() };
        return response;
    }

    public override async Task ServerStreaming(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        for (int i = 0; i < 100; i++)
        {
            await responseStream.WriteAsync(new Response { Message = "response " + i });
        }
    }

    public override async Task DuplexStreaming(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new Response { Message = "response " + request.Content });
        }
    }
}