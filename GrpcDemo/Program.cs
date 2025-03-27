using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using Auth;
using GrpcDemo.Interceptors;
using GrpcDemo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = JwtHelper.SecurityKey
        };
    }); 

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy => policy.RequireClaim(ClaimTypes.Name));
});
builder.Services.AddGrpcReflection();

builder.Services.AddGrpcHealthChecks().AddCheck("health check", () => HealthCheckResult.Healthy("OK"), ["gRPC Demo"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FirstService>();
app.MapGrpcHealthChecksService();
app.MapGrpcReflectionService(); // Allow to discover gRPC services using Postman
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

public partial class Program { }