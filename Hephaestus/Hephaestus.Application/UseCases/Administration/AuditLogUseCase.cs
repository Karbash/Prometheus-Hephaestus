using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

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
    /// Inicializa uma nova instância do <see cref="AuditLogUseCase"/>.
    /// </summary>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
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
    /// <param name="userId">ID do usuário (opcional).</param>
    /// <param name="startDate">Data inicial (opcional).</param>
    /// <param name="endDate">Data final (opcional).</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Lista de logs de auditoria.</returns>
    public async Task<IEnumerable<AuditLogResponse>> ExecuteAsync(string? userId, DateTime? startDate, DateTime? endDate, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação de autorização
            ValidateAuthorization(user);

            // Validação dos parâmetros
            await ValidateParametersAsync(userId, startDate, endDate);

            // Busca dos logs
            var logs = await GetLogsAsync(userId, startDate, endDate);

            // Conversão para DTOs de resposta
            return ConvertToResponseDtos(logs);
        });
    }

    /// <summary>
    /// Executa o registro de um log de auditoria.
    /// </summary>
    /// <param name="action">Ação realizada.</param>
    /// <param name="entityId">ID da entidade.</param>
    /// <param name="details">Detalhes da ação.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string action, string entityId, string details, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação de autorização
            ValidateAuthorizationForLogging(user);

            // Validação dos dados
            ValidateLogData(action, entityId, details);

            // Criação e registro do log
            await CreateAndSaveLogAsync(action, entityId, details, user);
        });
    }

    /// <summary>
    /// Valida a autorização para busca de logs.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        ValidateAuthorization(userRole == "Admin", "Apenas administradores podem acessar logs de auditoria.", "VIEW_AUDIT_LOGS", "AuditLog");
    }

    /// <summary>
    /// Valida a autorização para registro de logs.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateAuthorizationForLogging(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        ValidateAuthorization(userRole == "Admin" || userRole == "Tenant", "Apenas administradores e tenants podem registrar logs de auditoria.", "CREATE_AUDIT_LOG", "AuditLog");
    }

    /// <summary>
    /// Valida os parâmetros de busca.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
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
            throw new Hephaestus.Application.Exceptions.ValidationException("A data inicial não pode ser posterior à data final.", new ValidationResult());
    }

    /// <summary>
    /// Valida os dados do log.
    /// </summary>
    /// <param name="action">Ação realizada.</param>
    /// <param name="entityId">ID da entidade.</param>
    /// <param name="details">Detalhes da ação.</param>
    private void ValidateLogData(string action, string entityId, string details)
    {
        if (string.IsNullOrEmpty(action))
            throw new Hephaestus.Application.Exceptions.ValidationException("Ação é obrigatória.", new ValidationResult());

        if (string.IsNullOrEmpty(entityId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da entidade é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(details))
            throw new Hephaestus.Application.Exceptions.ValidationException("Detalhes são obrigatórios.", new ValidationResult());
    }

    /// <summary>
    /// Busca os logs de auditoria.
    /// </summary>
    /// <param name="userId">ID do usuário.</param>
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
    /// <param name="action">Ação realizada.</param>
    /// <param name="entityId">ID da entidade.</param>
    /// <param name="details">Detalhes da ação.</param>
    /// <param name="user">Usuário autenticado.</param>
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
}