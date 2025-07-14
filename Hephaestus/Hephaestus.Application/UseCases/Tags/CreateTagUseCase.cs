using FluentValidation.Results;
using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Interfaces.Tag;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Tag;

/// <summary>
/// Caso de uso para criação de tags.
/// </summary>
public class CreateTagUseCase : BaseUseCase, ICreateTagUseCase
{
    private readonly ITagRepository _tagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CreateTagUseCase"/>.
    /// </summary>
    /// <param name="tagRepository">Repositório de tags.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
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
    /// Executa a criação de uma tag.
    /// </summary>
    /// <param name="request">Dados da tag a ser criada.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Tag criada.</returns>
    public async Task<TagResponse> ExecuteAsync(TagRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            ValidateRequest(request);

            // Validação de autorização
            ValidateAuthorization(user);

            // Obtenção do tenant ID
            var tenantId = _loggedUserService.GetTenantId(user);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, tenantId);

            // Criação da tag
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
    /// Valida os dados da requisição.
    /// </summary>
    /// <param name="request">Requisição a ser validada.</param>
    private void ValidateRequest(TagRequest request)
    {
        if (request == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Dados da tag são obrigatórios.", new ValidationResult());

        if (string.IsNullOrEmpty(request.Name))
            throw new Hephaestus.Application.Exceptions.ValidationException("Nome da tag é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Valida a autorização do usuário.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
        {
            throw new UnauthorizedException("Apenas administradores ou tenants podem criar tags.", "CREATE_TAG", "Tag");
        }
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Requisição com os dados.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateBusinessRulesAsync(TagRequest request, string tenantId)
    {
        var existingTag = await _tagRepository.GetByNameAsync(request.Name, tenantId);
        if (existingTag != null)
        {
            throw new ConflictException("Tag já registrada para este tenant.", "Tag", "Name", request.Name);
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
    /// <param name="user">Usuário autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Tag tag, ClaimsPrincipal user)
    {
        var loggedUser = await _loggedUserService.GetLoggedUserAsync(user);
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = loggedUser.Id,
            Action = "Criação de Tag",
            EntityId = tag.Id,
            Details = $"Tag {tag.Name} criada para tenant {tag.TenantId}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}