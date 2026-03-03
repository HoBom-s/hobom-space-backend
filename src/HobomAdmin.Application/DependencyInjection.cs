using HobomAdmin.Application.Ports.In;
using HobomAdmin.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace HobomAdmin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetDlqKeysUseCase, GetDlqKeysUseCase>();
        services.AddScoped<IGetOutboxSummaryUseCase, GetOutboxSummaryUseCase>();
        return services;
    }
}
