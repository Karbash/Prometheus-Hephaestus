using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Application.Interfaces.Customer;
using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Application.UseCases.Administration;
using Hephaestus.Application.UseCases.Auth;
using Hephaestus.Application.UseCases.Customer;
using Hephaestus.Application.UseCases.Menu;
using Hephaestus.Application.Validators;
using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Microsoft.Extensions.DependencyInjection;

namespace Hephaestus.Application;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        AddUseCases(services);
        AddValidators(services);
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
        services.AddScoped<IGetCompaniesWithinRadiusUseCase, GetCompaniesWithinRadiusUseCase>();

        // Customer UseCases
        services.AddScoped<IUpdateCustomerUseCase, UpdateCustomerUseCase>();
        services.AddScoped<IGetCustomerUseCase, GetCustomerUseCase>();
        services.AddScoped<IGetByIdCustomerUseCase, GetByIdCustomerUseCase>();

        // Menu UseCases
        services.AddScoped<ICreateMenuItemUseCase, CreateMenuItemUseCase>();
        services.AddScoped<IGetMenuItemsUseCase, GetMenuItemsUseCase>();
        services.AddScoped<IGetMenuItemByIdUseCase, GetMenuItemByIdUseCase>();
        services.AddScoped<IUpdateMenuItemUseCase, UpdateMenuItemUseCase>();
        services.AddScoped<IDeleteMenuItemUseCase, DeleteMenuItemUseCase>();
    }

    private static void AddValidators(IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateMenuItemRequest>, CreateMenuItemRequestValidator>();
        services.AddScoped<IValidator<UpdateMenuItemRequest>, UpdateMenuItemRequestValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<MfaRequest>, MfaRequestValidator>();
        services.AddScoped<IValidator<RegisterCompanyRequest>, RegisterCompanyRequestValidator>();
        services.AddScoped<IValidator<ResetPasswordConfirmRequest>, ResetPasswordConfirmRequestValidator>();
        services.AddScoped<IValidator<ResetPasswordRequest>, ResetPasswordRequestValidator>();
        services.AddScoped<IValidator<UpdateCompanyRequest>, UpdateCompanyRequestValidator>();
    }
}