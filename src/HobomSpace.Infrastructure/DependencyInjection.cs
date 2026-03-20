using HobomSpace.Application.Ports;
using HobomSpace.Infrastructure.Persistence;
using HobomSpace.Infrastructure.Persistence.Interceptors;
using HobomSpace.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HobomSpace.Infrastructure;

/// <summary>Infrastructure 레이어 서비스를 DI 컨테이너에 등록한다.</summary>
public static class DependencyInjection
{
    /// <summary>DbContext, 레포지토리, UnitOfWork를 Scoped 수명주기로 등록한다.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DomainEventInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention()
                   .AddInterceptors(sp.GetRequiredService<DomainEventInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Generic Specification repos
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        // Extended repos (batch operations)
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IPageRepository, PageRepository>();

        return services;
    }
}
