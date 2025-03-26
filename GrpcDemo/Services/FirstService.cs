using System.Text;
using Grpc.Core;
using GrpcDemo.Protos;
namespace GrpcDemo.Services;

public class FirstService : FirstServiceDefinition.FirstServiceDefinitionBase
{
    public override Task<Response> UnaryDemo(Request request, ServerCallContext context)
    {
        // Added for testing retry scenarios
        /*if (!context.RequestHeaders.Any(x => x.Key == "grpc-previous-rpc-attempts"))
        {
            throw new RpcException(new Status(StatusCode.Unavailable, "try again"));
        }*/

        var response = new Response
        {
            Message = "Hello " + request.Content
        };
        
        context.WriteOptions = new WriteOptions(WriteFlags.NoCompress);
        
        return Task.FromResult(response);
    }

    public override async Task<Response> ClientStreamingDemo(IAsyncStreamReader<Request> requestStream, ServerCallContext context)
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

    public override async Task ServerStreamingDemo(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        for (int i = 0; i < 100; i++)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("server stream was cancelled on iteration:" + i);
                return;
            }

            await responseStream.WriteAsync(new Response { Message = "response " + i });
        }
        
        context.ResponseTrailers.Add(new Metadata.Entry("trailer-key", "trailer-value"));
    }

    public override async Task DuplexStreamingDemo(IAsyncStreamReader<Request> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            await responseStream.WriteAsync(new Response { Message = "response " + request.Content });
        }
    }
}