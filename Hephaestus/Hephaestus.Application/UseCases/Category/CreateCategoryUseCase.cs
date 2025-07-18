using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Category;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Hephaestus.Application.Exceptions;
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
            // Validação de autorização
            ValidateAuthorization(user);

            // Obtenção do role do usuário
            var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
            
            // Lógica híbrida: Admin cria categorias globais, Tenant cria categorias locais
            string? tenantId = null;
            if (userRole == "Tenant")
            {
                try
                {
                    tenantId = _loggedUserService.GetTenantId(user);
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException("TenantId não encontrado no token.");
                }
            }
            // Para Admin, tenantId permanece null (categoria global)

            await ValidateAsync(_validator, request);
            await ValidateBusinessRulesAsync(request, tenantId);

            var isGlobal = string.IsNullOrEmpty(tenantId);
            var category = new Domain.Entities.Category
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId ?? string.Empty, // Vazio para categorias globais
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                IsGlobal = isGlobal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            return category.Id;
        }, "CreateCategory");
    }

    /// <summary>
    /// Valida a autorização do usuário.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
        {
            throw new UnauthorizedException("Apenas administradores ou tenants podem criar categorias.", "CREATE_CATEGORY", "Category");
        }
    }

    private async Task ValidateBusinessRulesAsync(CreateCategoryRequest request, string? tenantId)
    {
        // Para categorias locais (tenant), verificar se já existe na empresa
        if (!string.IsNullOrEmpty(tenantId))
        {
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name, tenantId);
            EnsureBusinessRule(existingCategory == null, "Já existe uma categoria com este nome.", "CATEGORY_NAME_EXISTS");
        }
        
        // Para categorias globais (admin), verificar se já existe globalmente
        if (string.IsNullOrEmpty(tenantId))
        {
            var existingGlobalCategory = await _categoryRepository.GetByNameAsync(request.Name, null);
            EnsureBusinessRule(existingGlobalCategory == null, "Já existe uma categoria global com este nome.", "GLOBAL_CATEGORY_NAME_EXISTS");
        }
    }
} 
