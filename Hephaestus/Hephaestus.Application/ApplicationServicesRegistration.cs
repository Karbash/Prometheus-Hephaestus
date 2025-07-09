using Hephaestus.Application.Interfaces;
using Hephaestus.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Hephaestus.Application;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IRegisterCompanyUseCase, RegisterCompanyUseCase>();
        return services;
    }
}