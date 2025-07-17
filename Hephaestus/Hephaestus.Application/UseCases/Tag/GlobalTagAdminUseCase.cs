using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Linq;

namespace Hephaestus.Application.UseCases.Tag;

public class GlobalTagAdminUseCase : BaseUseCase, IGlobalTagAdminUseCase
{
    private readonly ITagRepository _tagRepository;

    public GlobalTagAdminUseCase(
        ITagRepository tagRepository,
        ILogger<GlobalTagAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _tagRepository = tagRepository;
    }

    public async Task<PagedResult<TagResponse>> ExecuteAsync(
        string? companyId = null,
        string? name = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedTags = await _tagRepository.GetAllGlobalAsync(name, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<TagResponse>
            {
                Items = pagedTags.Items.Select(t => new TagResponse
                {
                    Id = t.Id,
                    CompanyId = t.CompanyId,
                    Name = t.Name,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList(),
                TotalCount = pagedTags.TotalCount,
                PageNumber = pagedTags.PageNumber,
                PageSize = pagedTags.PageSize
            };
        });
    }
} 