using Hephaestus.Application.Interfaces.Menu;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Services;
using FluentValidation.Results;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Menu;

/// <summary>
/// Caso de uso para remoção de itens do cardápio.
/// </summary>
public class DeleteMenuItemUseCase : BaseUseCase, IDeleteMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="DeleteMenuItemUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Repositório de itens do cardápio.</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public DeleteMenuItemUseCase(
        IMenuItemRepository menuItemRepository,
        ILoggedUserService loggedUserService,
        ILogger<DeleteMenuItemUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _menuItemRepository = menuItemRepository;
        _loggedUserService = loggedUserService;
    }

    /// <summary>
    /// Executa a remoção de um item do cardápio.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var tenantId = _loggedUserService.GetTenantId(user);
            
            // Validação dos parâmetros de entrada
            ValidateInputParameters(id, tenantId);

            // Verifica se o item existe antes de tentar removê-lo
            await ValidateMenuItemExistsAsync(id, tenantId);

            // Remove o item do cardápio
            await _menuItemRepository.DeleteAsync(id, tenantId);
        });
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private void ValidateInputParameters(string id, string tenantId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do item do cardápio é obrigatório.", new ValidationResult());

        if (string.IsNullOrEmpty(tenantId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do tenant é obrigatório.", new ValidationResult());
    }

    /// <summary>
    /// Verifica se o item do cardápio existe.
    /// </summary>
    /// <param name="id">ID do item do cardápio.</param>
    /// <param name="tenantId">ID do tenant.</param>
    private async Task ValidateMenuItemExistsAsync(string id, string tenantId)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, tenantId);
        EnsureEntityExists(menuItem, "MenuItem", id);
    }
}
