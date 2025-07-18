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

            // Obtenção do role do usuário
            var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
            
            // Lógica híbrida: Admin cria tags globais, Tenant cria tags locais
            string? companyId = null;
            if (userRole == "Tenant")
            {
                try
                {
                    companyId = _loggedUserService.GetCompanyId(user);
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException("CompanyId não encontrado no token.");
                }
            }
            // Para Admin, companyId permanece null (tag global)

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, companyId);

            // Criação da tag
            var tag = await CreateTagEntityAsync(request, companyId);

            // Registro de auditoria
            await CreateAuditLogAsync(tag, user);

            // Mapeamento para resposta
            return new TagResponse
            {
                Id = tag.Id,
                CompanyId = tag.CompanyId,
                Name = tag.Name,
                Description = tag.Description,
                IsGlobal = tag.IsGlobal
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

        if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 500)
            throw new Hephaestus.Application.Exceptions.ValidationException("Descrição da tag deve ter no máximo 500 caracteres.", new ValidationResult());
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
    /// <param name="companyId">ID do tenant (null para tags globais).</param>
    private async Task ValidateBusinessRulesAsync(TagRequest request, string? companyId)
    {
        // Para tags locais (tenant), verificar se já existe na empresa
        if (!string.IsNullOrEmpty(companyId))
        {
            var existingTag = await _tagRepository.GetByNameAsync(request.Name, companyId);
            if (existingTag != null)
            {
                throw new ConflictException("Tag já registrada para este tenant.", "Tag", "Name", request.Name);
            }
        }
        
        // Para tags globais (admin), verificar se já existe globalmente
        if (string.IsNullOrEmpty(companyId))
        {
            var existingGlobalTag = await _tagRepository.GetByNameAsync(request.Name, null);
            if (existingGlobalTag != null)
            {
                throw new ConflictException("Tag global já registrada.", "Tag", "Name", request.Name);
            }
        }
    }

    /// <summary>
    /// Cria a entidade de tag.
    /// </summary>
    /// <param name="request">Dados da tag.</param>
    /// <param name="companyId">ID do tenant (null para tags globais).</param>
    /// <returns>Entidade de tag criada.</returns>
    private async Task<Domain.Entities.Tag> CreateTagEntityAsync(TagRequest request, string? companyId)
    {
        var isGlobal = string.IsNullOrEmpty(companyId);
        var tag = new Domain.Entities.Tag
        {
            Id = Guid.NewGuid().ToString(),
            CompanyId = companyId ?? string.Empty, // Vazio para tags globais
            Name = request.Name,
            Description = request.Description,
            IsGlobal = isGlobal
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
        var tagType = string.IsNullOrEmpty(tag.CompanyId) ? "global" : "local";
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = loggedUser.Id,
            Action = "Criação de Tag",
            EntityId = tag.Id,
            Details = $"Tag {tag.Name} criada como {tagType} {(string.IsNullOrEmpty(tag.CompanyId) ? "" : $"para empresa {tag.CompanyId}")}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}
