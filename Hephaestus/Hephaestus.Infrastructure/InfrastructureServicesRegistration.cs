using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Hephaestus.Infrastructure.Data;
using Hephaestus.Infrastructure.Repositories;
using Hephaestus.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hephaestus.Infrastructure;

public static class InfrastructureServicesRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
        AddRepositories(services);
        AddServices(services);
        return services;
    }

    private static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HephaestusDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection"), 
                npgsqlOptions => npgsqlOptions
                    .CommandTimeout(30) // Timeout de comando de 30 segundos
                    .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null) // 3 tentativas com delay de 5s
            ));
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ISalesRepository, SalesRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICompanyImageRepository, CompanyImageRepository>();
        services.AddScoped<ICompanyOperatingHourRepository, CompanyOperatingHourRepository>();
        services.AddScoped<ICompanySocialMediaRepository, CompanySocialMediaRepository>();
        services.AddScoped<IDatabaseRepository, DatabaseRepository>();
        services.AddScoped<IAdditionalRepository, AdditionalRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IMessageService, MessageService>();
        services.AddHttpClient<IMessageService, MessageService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "Hephaestus-API/1.0");
        });
        services.AddScoped<IMfaService, MfaService>();
        services.AddScoped<ILoggedUserService, LoggedUserService>();
    }
}