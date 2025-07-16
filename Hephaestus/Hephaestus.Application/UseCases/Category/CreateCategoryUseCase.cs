using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Category;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Category;

public class CreateCategoryUseCase : BaseUseCase, ICreateCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<CreateCategoryRequest> _validator;
    private readonly ILoggedUserService _loggedUserService;

    public CreateCategoryUseCase(
        ICategoryRepository categoryRepository,
        IValidator<CreateCategoryRequest> validator,
        ILoggedUserService loggedUserService,
        ILogger<CreateCategoryUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _categoryRepository = categoryRepository;
        _validator = validator;
        _loggedUserService = loggedUserService;
    }

    public async Task<string> ExecuteAsync(CreateCategoryRequest request, ClaimsPrincipal user)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);

            await ValidateAsync(_validator, request);
            await ValidateBusinessRulesAsync(request, tenantId);

            var category = new Domain.Entities.Category
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            return category.Id;
        }, "CreateCategory");
    }

    private async Task ValidateBusinessRulesAsync(CreateCategoryRequest request, string tenantId)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(request.Name, tenantId);
        EnsureBusinessRule(existingCategory == null, "Já existe uma categoria com este nome.", "CATEGORY_NAME_EXISTS");
    }
} 
