using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Linq;
using System.Collections.Generic;

namespace Hephaestus.Application.UseCases.Menu;

public class GlobalMenuItemAdminUseCase : BaseUseCase, IGlobalMenuItemAdminUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;

    public GlobalMenuItemAdminUseCase(
        IMenuItemRepository menuItemRepository,
        ILogger<GlobalMenuItemAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
    }

    public async Task<PagedResult<MenuItemResponse>> ExecuteAsync(
        string? companyId = null,
        List<string>? tagIds = null,
        List<string>? categoryIds = null,
        decimal? maxPrice = null,
        bool? isAvailable = null,
        bool? promotionActiveNow = null,
        int? promotionDayOfWeek = null,
        string? promotionTime = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedMenuItems = await _menuItemRepository.GetAllGlobalAsync(
                null, companyId, categoryIds?.FirstOrDefault(), isAvailable, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<MenuItemResponse>
            {
                Items = pagedMenuItems.Items.Select(m => new MenuItemResponse
                {
                    Id = m.Id,
                    CompanyId = m.CompanyId,
                    Name = m.Name,
                    Description = m.Description,
                    CategoryId = m.CategoryId,
                    Price = m.Price,
                    IsAvailable = m.IsAvailable,
                    ImageUrl = m.ImageUrl,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt,
                    Tags = m.MenuItemTags.Select(mt => new TagResponse {
                        Id = mt.Tag.Id,
                        Name = mt.Tag.Name,
                        Description = mt.Tag.Description,
                        CreatedAt = mt.Tag.CreatedAt,
                        UpdatedAt = mt.Tag.UpdatedAt
                    }).ToList(),
                    Additionals = m.MenuItemAdditionals.Select(a => new AdditionalResponse {
                        Id = a.Additional.Id,
                        Name = a.Additional.Name,
                        Price = a.Additional.Price,
                        Description = a.Additional.Description,
                        IsAvailable = a.Additional.IsAvailable,
                        CreatedBy = a.Additional.CreatedBy,
                        CreatedAt = a.Additional.CreatedAt,
                        UpdatedBy = a.Additional.UpdatedBy,
                        UpdatedAt = a.Additional.UpdatedAt
                    }).ToList()
                }).ToList(),
                TotalCount = pagedMenuItems.TotalCount,
                PageNumber = pagedMenuItems.PageNumber,
                PageSize = pagedMenuItems.PageSize
            };
        });
    }
} 