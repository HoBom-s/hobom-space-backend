using HobomSpace.Infrastructure.Persistence;
using HobomSpace.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Testcontainers.PostgreSql;

namespace HobomSpace.Tests.Integration;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>;

public class IntegrationTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public HttpClient HttpClient { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>((sp, options) =>
                options.UseNpgsql(_postgres.GetConnectionString())
                       .UseSnakeCaseNamingConvention()
                       .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>()));
        });

        builder.UseSetting("Security:ApiKey", "test-api-key");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "unused");
        builder.UseSetting("SkipMigration", "true");
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        HttpClient = CreateClient();
        HttpClient.DefaultRequestHeaders.Add("X-Api-Key", "test-api-key");

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
