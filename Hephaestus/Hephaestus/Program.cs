using Hephaestus.Application;
using Hephaestus.Infrastructure;
using Hephaestus.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

namespace Hephaestus;

/// <summary>
/// Configura��es da aplica��o principal.
/// </summary>
public static class Program
{
    /// <summary>
    /// Ponto de entrada da aplica��o.
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
    /// Configura os servi�os no cont�iner de inje��o de depend�ncia.
    /// </summary>
    /// <param name="services">Cole��o de servi�os.</param>
    /// <param name="configuration">Configura��o da aplica��o.</param>
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
    /// Configura a autentica��o JWT no cont�iner de servi�os.
    /// </summary>
    /// <param name="services">Cole��o de servi�os.</param>
    /// <param name="configuration">Configura��o da aplica��o.</param>
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
    /// Configura as pol�ticas de autoriza��o no cont�iner de servi�os.
    /// </summary>
    /// <param name="services">Cole��o de servi�os.</param>
    private static void ConfigureAuthorizationPolicies(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireMfa", policy =>
                policy.RequireClaim("MfaValidated", "true"));
        });
    }

    /// <summary>
    /// Configura o Swagger com suporte para autentica��o Bearer e documenta��o XML opcional.
    /// </summary>
    /// <param name="services">Cole��o de servi�os.</param>
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

            // Inclui documenta��o XML se o arquivo existir
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            // Configura autentica��o Bearer
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
    /// Configura o pipeline de middleware da aplica��o.
    /// </summary>
    /// <param name="app">Aplica��o web configurada.</param>
    private static void ConfigureMiddleware(WebApplication app)
    {
        // Middleware de tratamento global de exceções
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hephaestus API v1");
            options.RoutePrefix = string.Empty; // Swagger UI na raiz
        });

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
