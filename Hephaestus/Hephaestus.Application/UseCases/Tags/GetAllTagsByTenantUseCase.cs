using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
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
    private readonly ILoggedUserService _loggedUserService;

    public GetAllTagsByTenantUseCase(
        ITagRepository tagRepository,
        ILogger<GetAllTagsByTenantUseCase> logger,
        IExceptionHandlerService exceptionHandler,
        ILoggedUserService loggedUserService)
        : base(logger, exceptionHandler)
    {
        _tagRepository = tagRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<PagedResult<TagResponse>> ExecuteAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Obter tenantId do usuário logado
            var tenantId = _loggedUserService.GetTenantId(user);

            ValidateInputParameters(tenantId);
            var pagedTags = await _tagRepository.GetByTenantIdAsync(tenantId, pageNumber, pageSize, sortBy, sortOrder);
            if (!pagedTags.Items.Any())
            {
                throw new NotFoundException("Tags", tenantId);
            }
            return new PagedResult<TagResponse>
            {
                Items = pagedTags.Items.Select(t => new TagResponse
                {
                    Id = t.Id,
                    TenantId = t.TenantId,
                    Name = t.Name
                }).ToList(),
                TotalCount = pagedTags.TotalCount,
                PageNumber = pagedTags.PageNumber,
                PageSize = pagedTags.PageSize
            };
        });
    }

    private void ValidateInputParameters(string tenantId)
    {
        if (string.IsNullOrEmpty(tenantId))
            throw new ValidationException("ID do tenant é obrigatório.", new ValidationResult());
        if (!Guid.TryParse(tenantId, out _))
            throw new ValidationException("ID do tenant deve ser um GUID válido.", new ValidationResult());
    }

    private async Task<IEnumerable<Domain.Entities.Tag>> GetTagsAsync(string tenantId)
    {
        var pagedTags = await _tagRepository.GetByTenantIdAsync(tenantId);
        if (pagedTags == null || !pagedTags.Items.Any())
            throw new NotFoundException("Tags", tenantId);
        return pagedTags.Items;
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
