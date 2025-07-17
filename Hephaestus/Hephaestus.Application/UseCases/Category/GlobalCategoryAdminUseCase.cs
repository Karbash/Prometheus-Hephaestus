using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Linq;

namespace Hephaestus.Application.UseCases.Category;

public class GlobalCategoryAdminUseCase : BaseUseCase, IGlobalCategoryAdminUseCase
{
    private readonly ICategoryRepository _categoryRepository;

    public GlobalCategoryAdminUseCase(
        ICategoryRepository categoryRepository,
        ILogger<GlobalCategoryAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<CategoryResponse>> ExecuteAsync(
        string? companyId = null,
        string? name = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedCategories = await _categoryRepository.GetAllGlobalAsync(name, companyId, isActive, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<CategoryResponse>
            {
                Items = pagedCategories.Items.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    CompanyId = null, // Valor padrão, pois não existe na entidade
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