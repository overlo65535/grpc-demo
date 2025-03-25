using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace GrpcDemo.Client.Interceptors;

public class ClientRequestsLogger : Interceptor
{
    private readonly ILogger<ClientRequestsLogger> _logger;

    public ClientRequestsLogger(ILogger<ClientRequestsLogger> logger)
    {
        _logger = logger;
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        _logger.LogWarning($"BlockingUnaryCall request: {request}");
        
        return continuation(request, context);
    }
}