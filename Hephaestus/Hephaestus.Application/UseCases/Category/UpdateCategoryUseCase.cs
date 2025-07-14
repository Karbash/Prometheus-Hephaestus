using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Category;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Category;

public class UpdateCategoryUseCase : BaseUseCase, IUpdateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<UpdateCategoryRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    public UpdateCategoryUseCase(
        ICategoryRepository categoryRepository,
        IValidator<UpdateCategoryRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<UpdateCategoryUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _categoryRepository = categoryRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    public async Task ExecuteAsync(string id, UpdateCategoryRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);

            await ValidateAsync(_validator, request);
            await ValidateBusinessRulesAsync(request, tenantId, id);

            var category = await _categoryRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(category, "Category", id);

            await UpdateCategoryEntityAsync(category!, request);
        }, "UpdateCategory");
    }

    private async Task ValidateBusinessRulesAsync(UpdateCategoryRequest request, string tenantId, string categoryId)
    {
        if (!string.IsNullOrEmpty(request.Name))
        {
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name, tenantId);
            EnsureBusinessRule(existingCategory == null || existingCategory.Id == categoryId, 
                "Já existe uma categoria com este nome.", "CATEGORY_NAME_EXISTS");
        }
    }

    private async Task UpdateCategoryEntityAsync(Domain.Entities.Category category, UpdateCategoryRequest request)
    {
        category.Name = request.Name ?? category.Name;
        category.Description = request.Description ?? category.Description;
        category.IsActive = request.IsActive ?? category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);
    }
} 