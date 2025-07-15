using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Category;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Category;

public class GetCategoriesUseCase : BaseUseCase, IGetCategoriesUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggedUserService _loggedUserService;

    public GetCategoriesUseCase(
        ICategoryRepository categoryRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetCategoriesUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _categoryRepository = categoryRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<PagedResult<CategoryResponse>> ExecuteAsync(ClaimsPrincipal user, int pageNumber = 1, int pageSize = 20, string? sortBy = null, string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            var pagedCategories = await _categoryRepository.GetByTenantIdAsync(tenantId, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<CategoryResponse>
            {
                Items = pagedCategories.Items.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    TenantId = c.TenantId,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList(),
                TotalCount = pagedCategories.TotalCount,
                PageNumber = pagedCategories.PageNumber,
                PageSize = pagedCategories.PageSize
            };
        });
    }
} 