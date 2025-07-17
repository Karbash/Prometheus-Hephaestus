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
/// Caso de uso para exclus�o de tags.
/// </summary>
public class DeleteTagUseCase : BaseUseCase, IDeleteTagUseCase
{
    private readonly ITagRepository _tagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="DeleteTagUseCase"/>.
    /// </summary>
    /// <param name="tagRepository">Reposit�rio de tags.</param>
    /// <param name="auditLogRepository">Reposit�rio de logs de auditoria.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a exclus�o de uma tag.
    /// </summary>
    /// <param name="id">ID da tag a ser exclu�da.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos par�metros de entrada
            ValidateInputParameters(id, user);

            // Valida��o de autoriza��o
            ValidateAuthorization(user);

            // Busca e valida��o da tag
            var tag = await GetAndValidateTagAsync(id, user);

            // Registro de auditoria
            await CreateAuditLogAsync(tag, user);

            // Exclus�o da tag
            await DeleteTagAsync(id, tag.CompanyId);
        });
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    private void ValidateInputParameters(string id, ClaimsPrincipal user)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da tag � obrigat�rio.", new ValidationResult());

        if (user == null)
            throw new Hephaestus.Application.Exceptions.ValidationException("Usu�rio autenticado � obrigat�rio.", new ValidationResult());

        if (!Guid.TryParse(id, out _))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da tag deve ser um GUID v�lido.", new ValidationResult());
    }

    /// <summary>
    /// Valida a autoriza��o do usu�rio.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
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
    /// <param name="user">Usu�rio autenticado.</param>
    /// <returns>Tag encontrada.</returns>
    private async Task<Domain.Entities.Tag> GetAndValidateTagAsync(string id, ClaimsPrincipal user)
    {
        var companyId = _loggedUserService.GetCompanyId(user);

        var tag = await _tagRepository.GetByIdAsync(id, companyId);
        if (tag == null)
            throw new NotFoundException("Tag", id);

        // Verifica se o usu�rio tenant est� tentando excluir tag de outro tenant
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole == "Tenant" && companyId != tag.CompanyId)
            throw new UnauthorizedException("Tenants s podem excluir suas prprias tags.", "DELETE_TAG", "Tag");

        return tag;
    }

    /// <summary>
    /// Registra o log de auditoria.
    /// </summary>
    /// <param name="tag">Tag que ser� exclu�da.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Tag tag, ClaimsPrincipal user)
    {
        var loggedUser = await _loggedUserService.GetLoggedUserAsync(user);
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            UserId = loggedUser.Id,
            Action = "DELETE_TAG",
            EntityId = tag.Id,
            Details = $"Tag '{tag.Name}' exclu�da",
            CreatedAt = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog);
    }

    /// <summary>
    /// Exclui a tag do reposit�rio.
    /// </summary>
    /// <param name="id">ID da tag.</param>
    /// <param name="companyId">ID do tenant.</param>
    private async Task DeleteTagAsync(string id, string companyId)
    {
        await _tagRepository.DeleteAsync(id, companyId);
    }
}
