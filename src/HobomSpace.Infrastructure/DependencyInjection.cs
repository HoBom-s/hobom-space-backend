using HobomSpace.Application.Ports;
using HobomSpace.Infrastructure.Persistence;
using HobomSpace.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HobomSpace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISpaceRepository, SpaceRepository>();
        services.AddScoped<IPageRepository, PageRepository>();

        return services;
    }
}
