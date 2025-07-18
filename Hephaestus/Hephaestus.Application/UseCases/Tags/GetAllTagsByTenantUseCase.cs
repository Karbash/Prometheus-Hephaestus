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
            var companyId = _loggedUserService.GetCompanyId(user);

            ValidateInputParameters(companyId);
            
            // Usar busca híbrida: tags locais + tags globais
            var pagedTags = await _tagRepository.GetHybridTagsAsync(companyId, pageNumber, pageSize, sortBy, sortOrder);
            
            if (!pagedTags.Items.Any())
            {
                throw new NotFoundException("Tags", companyId);
            }
            
            return new PagedResult<TagResponse>
            {
                Items = pagedTags.Items.Select(t => new TagResponse
                {
                    Id = t.Id,
                    CompanyId = t.CompanyId,
                    Name = t.Name,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    IsGlobal = t.IsGlobal // Usar a propriedade da entidade
                }).ToList(),
                TotalCount = pagedTags.TotalCount,
                PageNumber = pagedTags.PageNumber,
                PageSize = pagedTags.PageSize
            };
        });
    }

    private void ValidateInputParameters(string companyId)
    {
        if (string.IsNullOrEmpty(companyId))
            throw new ValidationException("ID da empresa é obrigatório.", new ValidationResult());
        if (!Guid.TryParse(companyId, out _))
            throw new ValidationException("ID da empresa inválido.", new ValidationResult());
    }

    private async Task<IEnumerable<Domain.Entities.Tag>> GetTagsAsync(string companyId)
    {
        var pagedTags = await _tagRepository.GetByCompanyIdAsync(companyId);
        if (pagedTags == null || !pagedTags.Items.Any())
            throw new NotFoundException("Tags", companyId);
        return pagedTags.Items;
    }

    private IEnumerable<TagResponse> ConvertToResponseDtos(IEnumerable<Domain.Entities.Tag> tags)
    {
        return tags.Select(tag => new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name,
            CompanyId = tag.CompanyId
        });
    }
}
