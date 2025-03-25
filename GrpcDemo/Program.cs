using System.IO.Compression;
using GrpcDemo.Interceptors;
using GrpcDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<RequestLogger>();
    options.ResponseCompressionAlgorithm = "gzip";
    options.ResponseCompressionLevel = CompressionLevel.SmallestSize;
    // options.CompressionProviders
});
builder.Services.AddLogging(configure => configure.AddConsole());


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FirstService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();