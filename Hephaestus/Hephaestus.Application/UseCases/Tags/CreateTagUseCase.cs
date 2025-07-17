using FluentValidation.Results;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Tag;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Hephaestus.Domain.DTOs.Request;

namespace Hephaestus.Application.UseCases.Tag;

/// <summary>
/// Caso de uso para cria��o de tags.
/// </summary>
public class CreateTagUseCase : BaseUseCase, ICreateTagUseCase
{
    private readonly ITagRepository _tagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="CreateTagUseCase"/>.
    /// </summary>
    /// <param name="tagRepository">Reposit�rio de tags.</param>
    /// <param name="auditLogRepository">Reposit�rio de logs de auditoria.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    public CreateTagUseCase(
        ITagRepository tagRepository,
        IAuditLogRepository auditLogRepository,
        ILoggedUserService loggedUserService,
        ILogger<CreateTagUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _tagRepository = tagRepository;
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a cria��o de uma tag.
    /// </summary>
    /// <param name="request">Dados da tag a ser criada.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Tag criada.</returns>
    public async Task<TagResponse> ExecuteAsync(TagRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            ValidateRequest(request);

            // Valida��o de autoriza��o
            ValidateAuthorization(user);

            // Obten��o do tenant ID
            var tenantId = _loggedUserService.GetTenantId(user);

            // Valida��o das regras de neg�cio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Cria��o da tag
            var tag = await CreateTagEntityAsync(request, tenantId);

            // Registro de auditoria
            await CreateAuditLogAsync(tag, user);

            // Mapeamento para resposta
            return new TagResponse
            {
                Id = tag.Id,
                TenantId = tag.TenantId,
                Name = tag.Name
            };
        });
    }

    /// <summary>
    /// Valida os dados da requisi��o.
    /// </summary>
    /// <param name="request">Requisi��o a ser validada.</param>
    private void ValidateRequest(TagRequest request)
    {
        if (request == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da tag s�o obrigat�rios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Name))
            throw new Hephaestus.Application.Exceptions.ValidationException("Nome da tag � obrigat�rio.", new ValidationResult());
    }

    /// <summary>
    /// Valida a autoriza��o do usu�rio.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
        {
            throw new UnauthorizedException("Apenas administradores ou tenants podem criar tags.", "CREATE_TAG", "Tag");
        }
    }

    /// <summary>
    /// Valida as regras de neg�cio.
    /// </summary>
    /// <param name="request">Requisi��o com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(TagRequest request, string tenantId)
    {
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, tenantId);
        if (existingTag != null)
        {
            throw new ConflictException("Tag j� registrada para este tenant.", "Tag", "Name", request.Name);
        }
    }

    /// <summary>
    /// Cria a entidade de tag.
    /// </summary>
    /// <param name="request">Dados da tag.</param>
    /// <param name="tenantId">ID do tenant.</param>
    /// <returns>Entidade de tag criada.</returns>
    private async Task<Domain.Entities.Tag> CreateTagEntityAsync(TagRequest request, string tenantId)
    {
        var tag = new Domain.Entities.Tag
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name
        };

        await _tagRepository.AddAsync(tag);
        return tag;
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="tag">Tag criada.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Tag tag, ClaimsPrincipal user)
    {
        var loggedUser = await _loggedUserService.GetLoggedUserAsync(user);
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = loggedUser.Id,
            Action = "Cria��o de Tag",
            EntityId = tag.Id,
            Details = $"Tag {tag.Name} criada para tenant {tag.TenantId}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}
