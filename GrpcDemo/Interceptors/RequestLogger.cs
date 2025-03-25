using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcDemo.Interceptors;

public class RequestLogger : Interceptor
{
    private readonly ILogger<RequestLogger> _logger;

    public RequestLogger(ILogger<RequestLogger> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogWarning($"UnaryServerHandler<TRequest, TResponse> request: {request}");
        return await continuation(request, context);
    }
}