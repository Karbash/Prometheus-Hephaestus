using FluentValidation.Results;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para logs de auditoria.
/// </summary>
public class AuditLogUseCase : BaseUseCase, IAuditLogUseCase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="AuditLogUseCase"/>.
    /// </summary>
    /// <param name="auditLogRepository">Reposit�rio de logs de auditoria.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public AuditLogUseCase(
        IAuditLogRepository auditLogRepository, 
        ILoggedUserService loggedUserService, 
        ICompanyRepository companyRepository,
        ILogger<AuditLogUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Executa a busca de logs de auditoria.
    /// </summary>
    /// <param name="startDate">Data inicial (opcional).</param>
    /// <param name="endDate">Data final (opcional).</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Lista de logs de auditoria.</returns>
    public async Task<IEnumerable<AuditLogResponse>> ExecuteAsync(DateTime? startDate, DateTime? endDate, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o de autoriza��o
            ValidateAuthorization(user);

            // Obter adminId do usu�rio logado (se aplic�vel)
            var adminId = GetAdminIdIfApplicable(user);

            // Valida��o dos par�metros
            await ValidateParametersAsync(adminId, startDate, endDate);

            // Busca dos logs
            var logs = await GetLogsAsync(adminId, startDate, endDate);

            // Convers�o para DTOs de resposta
            return ConvertToResponseDtos(logs);
        });
    }

    /// <summary>
    /// Executa o registro de um log de auditoria.
    /// </summary>
    /// <param name="action">A��o realizada.</param>
    /// <param name="entityId">ID da entidade.</param>
    /// <param name="details">Detalhes da a��o.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string action, string entityId, string details, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o de autoriza��o
            ValidateAuthorizationForLogging(user);

            // Valida��o dos dados
            ValidateLogData(action, entityId, details);

            // Cria��o e registro do log
            await CreateAndSaveLogAsync(action, entityId, details, user);
        });
    }

    /// <summary>
    /// Valida a autoriza��o para busca de logs.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        ValidateAuthorization(userRole == "Admin", "Apenas administradores podem acessar logs de auditoria.", "VIEW_AUDIT_LOGS", "AuditLog");
    }

    /// <summary>
    /// Valida a autoriza��o para registro de logs.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    private void ValidateAuthorizationForLogging(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        ValidateAuthorization(userRole == "Admin" || userRole == "Tenant", "Apenas administradores e tenants podem registrar logs de auditoria.", "CREATE_AUDIT_LOG", "AuditLog");
    }

    /// <summary>
    /// Valida os par�metros de busca.
    /// </summary>
    /// <param name="userId">ID do usu�rio.</param>
    /// <param name="startDate">Data inicial.</param>
    /// <param name="endDate">Data final.</param>
    private async Task ValidateParametersAsync(string? userId, DateTime? startDate, DateTime? endDate)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            var company = await _companyRepository.GetByIdAsync(userId);
            EnsureEntityExists(company, "Company", userId);
        }

        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            throw new Hephaestus.Application.Exceptions.ValidationException("A data inicial n�o pode ser posterior � data final.", new ValidationResult());
    }

    /// <summary>
    /// Valida os dados do log.
    /// </summary>
    /// <param name="action">A��o realizada.</param>
    /// <param name="entityId">ID da entidade.</param>
    /// <param name="details">Detalhes da a��o.</param>
    private void ValidateLogData(string action, string entityId, string details)
    {
        if (string.IsNullOrEmpty(action))
            throw new Hephaestus.Application.Exceptions.ValidationException("A��o � obrigat�ria.", new ValidationResult());

        if (string.IsNullOrEmpty(entityId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da entidade � obrigat�rio.", new ValidationResult());

        if (string.IsNullOrEmpty(details))
            throw new Hephaestus.Application.Exceptions.ValidationException("Detalhes s�o obrigat�rios.", new ValidationResult());
    }

    /// <summary>
    /// Busca os logs de auditoria.
    /// </summary>
    /// <param name="userId">ID do usu�rio.</param>
    /// <param name="startDate">Data inicial.</param>
    /// <param name="endDate">Data final.</param>
    /// <returns>Lista de logs.</returns>
    private async Task<IEnumerable<AuditLog>> GetLogsAsync(string? userId, DateTime? startDate, DateTime? endDate)
    {
        var normalizedStartDate = startDate?.ToUniversalTime();
        var normalizedEndDate = endDate?.ToUniversalTime();

        return await _auditLogRepository.GetLogsAsync(userId, normalizedStartDate, normalizedEndDate);
    }

    /// <summary>
    /// Converte as entidades para DTOs de resposta.
    /// </summary>
    /// <param name="logs">Lista de logs.</param>
    /// <returns>Lista de DTOs de resposta.</returns>
    private IEnumerable<AuditLogResponse> ConvertToResponseDtos(IEnumerable<AuditLog> logs)
    {
        return logs.Select(l => new AuditLogResponse
        {
            Id = l.Id,
            UserId = l.UserId,
            Action = l.Action,
            EntityId = l.EntityId,
            Details = l.Details,
            CreatedAt = l.CreatedAt
        });
    }

    /// <summary>
    /// Cria e salva o log de auditoria.
    /// </summary>
    /// <param name="action">A��o realizada.</param>
    /// <param name="entityId">ID da entidade.</param>
    /// <param name="details">Detalhes da a��o.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    private async Task CreateAndSaveLogAsync(string action, string entityId, string details, ClaimsPrincipal user)
    {
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        var tenantId = userRole == "Tenant" ? user?.FindFirst("TenantId")?.Value ?? string.Empty : null;

        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            TenantId = tenantId,
            Action = action,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    /// <summary>
    /// Obt�m o adminId se o usu�rio for um admin, caso contr�rio retorna null.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>AdminId ou null.</returns>
    private string? GetAdminIdIfApplicable(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Admin")
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        return null; // Apenas admins podem ver logs de auditoria
    }
}
