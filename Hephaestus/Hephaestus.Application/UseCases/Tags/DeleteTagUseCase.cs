using Hephaestus.Application.Interfaces.Tag;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System.Security.Claims;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Tag;

/// <summary>
/// Caso de uso para exclusão de tags.
/// </summary>
public class DeleteTagUseCase : BaseUseCase, IDeleteTagUseCase
{
    private readonly ITagRepository _tagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="DeleteTagUseCase"/>.
    /// </summary>
    /// <param name="tagRepository">Repositório de tags.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public DeleteTagUseCase(
        ITagRepository tagRepository,
        IAuditLogRepository auditLogRepository,
        ILoggedUserService loggedUserService,
        ILogger<DeleteTagUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _tagRepository = tagRepository;
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a exclusão de uma tag.
    /// </summary>
    /// <param name="id">ID da tag a ser excluída.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(id, user);

            // Validação de autorização
            ValidateAuthorization(user);

            // Busca e validação da tag
            var tag = await GetAndValidateTagAsync(id, user);

            // Registro de auditoria
            await CreateAuditLogAsync(tag, user);

            // Exclusão da tag
            await DeleteTagAsync(id, tag.TenantId);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateInputParameters(string id, ClaimsPrincipal user)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da tag é obrigatório.", new ValidationResult());

        if (user == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Usuário autenticado é obrigatório.", new ValidationResult());

        if (!Guid.TryParse(id, out _))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da tag deve ser um GUID válido.", new ValidationResult());
    }

    /// <summary>
    /// Valida a autorização do usuário.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
            throw new UnauthorizedException("Apenas administradores ou tenants podem excluir tags.", "DELETE_TAG", "Tag");
    }

    /// <summary>
    /// Busca e valida a tag.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <param name="user">Usuário autenticado.</param>
    /// <returns>Tag encontrada.</returns>
    private async Task<Domain.Entities.Tag> GetAndValidateTagAsync(string id, ClaimsPrincipal user)
    {
        var tenantId = _loggedUserService.GetTenantId(user);

        var tag = await _tagRepository.GetByIdAsync(id, tenantId);
        if (tag == null)
            throw new NotFoundException("Tag", id);

        // Verifica se o usuário tenant está tentando excluir tag de outro tenant
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Tenant" && tenantId != tag.TenantId)
            throw new UnauthorizedException("Tenants só podem excluir suas próprias tags.", "DELETE_TAG", "Tag");

        return tag;
    }

    /// <summary>
    /// Registra o log de auditoria.
    /// </summary>
    /// <param name="tag">Tag que será excluída.</param>
    /// <param name="user">Usuário autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Tag tag, ClaimsPrincipal user)
    {
        var loggedUser = await _loggedUserService.GetLoggedUserAsync(user);
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = loggedUser.Id,
            Action = "DELETE_TAG",
            EntityId = tag.Id,
            Details = $"Tag '{tag.Name}' excluída",
            CreatedAt = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    /// <summary>
    /// Exclui a tag do repositório.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task DeleteTagAsync(string id, string tenantId)
    {
        await _tagRepository.DeleteAsync(id, tenantId);
    }
}
