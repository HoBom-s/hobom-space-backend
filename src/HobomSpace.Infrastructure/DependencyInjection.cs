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
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ISpaceRepository, SpaceRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IPageVersionRepository, PageVersionRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        return services;
    }
}
