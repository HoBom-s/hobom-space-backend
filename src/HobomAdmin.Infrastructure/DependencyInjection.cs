using HobomAdmin.Application.Ports.Out;
using HobomAdmin.Infrastructure.Adapters.MongoDb;
using HobomAdmin.Infrastructure.Adapters.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;

namespace HobomAdmin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddScoped<IDlqReader, RedisDlqReader>();

        // MongoDB
        var mongoConnection = configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
        var mongoDatabase = configuration["MongoDb:Database"] ?? "hobom";
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
        services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabase));
        services.AddScoped<IOutboxReader, MongoOutboxReader>();

        return services;
    }
}
