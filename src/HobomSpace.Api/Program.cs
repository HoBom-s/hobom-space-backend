using System.Threading.RateLimiting;
using HobomSpace.Api.BackgroundServices;
using HobomSpace.Api.Endpoints;
using HobomSpace.Api.Grpc;
using HobomSpace.Api.Middleware;
using HobomSpace.Application;
using HobomSpace.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

    builder.Host.UseSerilog((context, config) => config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "HobomSpace")
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {TraceId} {Message:lj}{NewLine}{Exception}"));

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            var components = document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
            components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
            components.SecuritySchemes["ApiKey"] = new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
                Name = "X-Api-Key",
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Description = "API key passed via X-Api-Key header",
            };
            document.Security ??= [];
            document.Security.Add(new Microsoft.OpenApi.OpenApiSecurityRequirement
            {
                [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
            });
            return Task.CompletedTask;
        });
    });
    builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<ApiKeyInterceptor>();
    });
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHostedService<OutboxCleanupService>();

    var healthChecks = builder.Services.AddHealthChecks();
    var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(dbConnectionString))
        healthChecks.AddNpgSql(dbConnectionString);

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("fixed", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
    });

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
        options.ListenAnyIP(8080);

        var tlsCert = builder.Configuration["GrpcTls:CertPath"];
        var tlsKey = builder.Configuration["GrpcTls:KeyPath"];

        if (!string.IsNullOrEmpty(tlsCert) && !string.IsNullOrEmpty(tlsKey)
            && System.IO.File.Exists(tlsCert) && System.IO.File.Exists(tlsKey))
        {
            options.ListenAnyIP(50052, o =>
            {
                o.Protocols = HttpProtocols.Http2;
                o.UseHttps(System.Security.Cryptography.X509Certificates.X509Certificate2
                    .CreateFromPemFile(tlsCert, tlsKey));
            });
        }
        else
        {
            options.ListenAnyIP(50052, o => o.Protocols = HttpProtocols.Http2);
        }
    });

    builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(30));

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<ApiKeyMiddleware>();

    app.UseMiddleware<ApiLoggingMiddleware>();
    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
    app.UseHttpsRedirection();

    app.UseCors();
    app.UseRateLimiter();

    app.MapOpenApi();
    app.MapGet("/scalar/v1", () => Results.Content("""
        <!doctype html>
        <html>
        <head><title>HobomSpace API</title><meta charset="utf-8" /></head>
        <body>
            <script id="api-reference" data-url="../openapi/v1.json"></script>
            <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
        </body>
        </html>
        """, "text/html")).ExcludeFromDescription();

    app.MapHealthChecks("/health/ready");
    app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));
    app.MapSpaceEndpoints().RequireRateLimiting("fixed");
    app.MapPageEndpoints().RequireRateLimiting("fixed");
    app.MapPageVersionEndpoints().RequireRateLimiting("fixed");
    app.MapCommentEndpoints().RequireRateLimiting("fixed");
    app.MapSearchEndpoints().RequireRateLimiting("fixed");
    app.MapErrorEndpoints().RequireRateLimiting("fixed");
    app.MapGrpcService<SpaceOutboxFindService>();
    app.MapGrpcService<SpaceOutboxPatchService>();
    app.MapGrpcService<SpaceLogFindService>();

    Log.Information("Starting HobomSpace API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
