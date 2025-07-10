using Hephaestus.Application;
using Hephaestus.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Hephaestus;

/// <summary>
/// Configurações da aplicação principal.
/// </summary>
public static class Program
{
    /// <summary>
    /// Ponto de entrada da aplicação.
    /// </summary>
    /// <param name="args">Argumentos da linha de comando.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        ConfigureMiddleware(app);

        app.Run();
    }

    /// <summary>
    /// Configura os serviços no contêiner de injeção de dependência.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Adiciona logging
        services.AddLogging(logging => logging.AddConsole());

        // Adiciona serviços
        services.AddControllers();
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);

        // Configura autenticação JWT
        ConfigureJwtAuthentication(services, configuration);

        // Configura políticas de autorização
        ConfigureAuthorizationPolicies(services);

        // Configura Swagger
        ConfigureSwagger(services);
    }

    /// <summary>
    /// Configura a autenticação JWT no contêiner de serviços.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    private static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };
        });
    }

    /// <summary>
    /// Configura as políticas de autorização no contêiner de serviços.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    private static void ConfigureAuthorizationPolicies(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireMfa", policy =>
                policy.RequireClaim("MfaValidated", "true"));
        });
    }

    /// <summary>
    /// Configura o Swagger com suporte para autenticação Bearer e documentação XML opcional.
    /// </summary>
    /// <param name="services">Coleção de serviços.</param>
    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Hephaestus API",
                Version = "v1",
                Description = "API para gerenciamento de pedidos."
            });

            // Inclui documentação XML se o arquivo existir
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            // Configura autenticação Bearer
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Bearer {token}\""
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    /// <summary>
    /// Configura o pipeline de middleware da aplicação.
    /// </summary>
    /// <param name="app">Aplicação web configurada.</param>
    private static void ConfigureMiddleware(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hephaestus API v1");
                options.RoutePrefix = string.Empty; // Swagger UI na raiz
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}