using HobomSpace.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HobomSpace.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISpaceService, SpaceService>();
        services.AddScoped<IPageService, PageService>();
        services.AddScoped<IPageVersionService, PageVersionService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ISearchService, SearchService>();

        return services;
    }
}
