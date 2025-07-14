using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Category;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Category;

public class GetCategoryByIdUseCase : BaseUseCase, IGetCategoryByIdUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggedUserService _loggedUserService;

    public GetCategoryByIdUseCase(
        ICategoryRepository categoryRepository,
        ILoggedUserService loggedUserService,
        ILogger<GetCategoryByIdUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _categoryRepository = categoryRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<CategoryResponse?> ExecuteAsync(string id, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);

            var category = await _categoryRepository.GetByIdAsync(id, tenantId);
            if (category == null)
                return null;

            return new CategoryResponse
            {
                Id = category.Id,
                TenantId = category.TenantId,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }, "GetCategoryById");
    }
} 