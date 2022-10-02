using System.IO.Compression;

using Grpc.Core;
using Grpc.Net.Compression;
using Calzolari.Grpc.AspNetCore.Validation;

using CountryService.Web.Interceptors;
using CountryService.Web.Providers;
using CountryService.Web.Services;
using CountryService.Web.Validators;

using v1 = CountryService.Web.Services.v1;
using v2 = CountryService.Web.Services.v2;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
    options.CompressionProviders = new List<ICompressionProvider>
    {
        new GzipCompressionProvider(CompressionLevel.Optimal),
        new BrotliCompressionProvider(CompressionLevel.Optimal)
    };
    options.ResponseCompressionAlgorithm = "br";
    options.ResponseCompressionLevel = CompressionLevel.Optimal;

    // Register custom ExceptionInterceptor interceptor
    options.Interceptors.Add<ExceptionInterceptor>();

    options.EnableMessageValidation();
});
builder.Services.AddGrpcValidation();
builder.Services.AddValidator<CountryCreateRequestValidator>();

builder.Services.AddGrpcReflection();
builder.Services.AddSingleton<CountryManagementService>();

var app = builder.Build();


// Configure the HTTP request pipeline.

app.MapGrpcReflectionService();

app.MapGrpcService<v1.CountryGrpcService>();
app.MapGrpcService<v2.CountryGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
