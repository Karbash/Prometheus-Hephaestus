using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Category;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Category;

public class DeleteCategoryUseCase : BaseUseCase, IDeleteCategoryUseCase
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggedUserService _loggedUserService;

    public DeleteCategoryUseCase(
        ICategoryRepository categoryRepository,
        ILoggedUserService loggedUserService,
        ILogger<DeleteCategoryUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _categoryRepository = categoryRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);

            var category = await _categoryRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(category, "Category", id);

            await _categoryRepository.DeleteAsync(id, tenantId);
        }, "DeleteCategory");
    }
} 
