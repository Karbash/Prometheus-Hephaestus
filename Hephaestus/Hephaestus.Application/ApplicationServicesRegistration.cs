using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Application.UseCases.Administration;
using Hephaestus.Application.UseCases.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Hephaestus.Application;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        AddUseCases(services);
        return services;
    }

    private static void AddUseCases(IServiceCollection services)
    {
        // Auth UseCases
        services.AddScoped<ILoginUseCase, LoginUseCase>();
        services.AddScoped<IRegisterCompanyUseCase, RegisterCompanyUseCase>();
        services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
        services.AddScoped<IMfaUseCase, MfaUseCase>();

        // Administration UseCases
        services.AddScoped<IGetCompaniesUseCase, GetCompaniesUseCase>();
        services.AddScoped<IUpdateCompanyUseCase, UpdateCompanyUseCase>();
        services.AddScoped<ISalesReportUseCase, SalesReportUseCase>();
        services.AddScoped<IAuditLogUseCase, AuditLogUseCase>();
    }
}