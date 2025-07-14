using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using FluentValidation.Results;
using Hephaestus.Application.Interfaces.Tag;

namespace Hephaestus.Application.UseCases.Tags;

/// <summary>
/// Caso de uso para obter todas as tags de um tenant.
/// </summary>
public class GetAllTagsByTenantUseCase : BaseUseCase, IGetAllTagsByTenantUseCase
{
    private readonly ITagRepository _tagRepository;

    public GetAllTagsByTenantUseCase(
        ITagRepository tagRepository,
        ILogger<GetAllTagsByTenantUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<TagResponse>> ExecuteAsync(string tenantId, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            ValidateInputParameters(tenantId, user);
            ValidateAuthorization(user);
            var tags = await GetTagsAsync(tenantId);
            return ConvertToResponseDtos(tags);
        });
    }

    private void ValidateInputParameters(string tenantId, ClaimsPrincipal user)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ValidationException("ID do tenant é obrigatório.", new ValidationResult());
        if (user == null)
            throw new ValidationException("Usuário autenticado é obrigatório.", new ValidationResult());
        if (!Guid.TryParse(tenantId, out _))
            throw new ValidationException("ID do tenant deve ser um GUID válido.", new ValidationResult());
    }

    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
            throw new UnauthorizedException("Apenas administradores ou tenants podem visualizar tags.", "GET_TAGS", "Tag");
        var userTenantId = user.FindFirst("TenantId")?.Value;
        var tenantId = user.FindFirst("TenantId")?.Value;
        if (userRole == "Tenant" && userTenantId != tenantId)
            throw new UnauthorizedException("Tenants só podem visualizar suas próprias tags.", "GET_TAGS", "Tag");
    }

    private async Task<IEnumerable<Domain.Entities.Tag>> GetTagsAsync(string tenantId)
    {
        var tags = await _tagRepository.GetByTenantIdAsync(tenantId);
        if (tags == null || !tags.Any())
            throw new NotFoundException("Tags", tenantId);
        return tags;
    }

    private IEnumerable<TagResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.Tag> tags)
    {
        return tags.Select(tag => new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            TenantId = tag.TenantId
        });
    }
}