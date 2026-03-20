using HobomSpace.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HobomSpace.Application;

/// <summary>Application 레이어 서비스를 DI 컨테이너에 등록한다.</summary>
public static class DependencyInjection
{
    /// <summary>Application 서비스를 Scoped 수명주기로 등록한다.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISpaceService, SpaceService>();
        services.AddScoped<IPageService, PageService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<ITrashService, TrashService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IVersionService, VersionService>();
        services.AddScoped<IErrorService, ErrorService>();

        return services;
    }
}
