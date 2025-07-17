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
/// Caso de uso para remo��o de itens do card�pio.
/// </summary>
public class DeleteMenuItemUseCase : BaseUseCase, IDeleteMenuItemUseCase
{
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly ILoggedUserService _loggedUserService;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="DeleteMenuItemUseCase"/>.
    /// </summary>
    /// <param name="menuItemRepository">Reposit�rio de itens do card�pio.</param>
    /// <param name="loggedUserService">Servi�o para obter informa��es do usu�rio logado.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a remo��o de um item do card�pio.
    /// </summary>
    /// <param name="id">ID do item do card�pio.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var companyId = _loggedUserService.GetCompanyId(user);
            
            // Validao dos par�metros de entrada
            ValidateInputParameters(id, companyId);

            // Verifica se o item existe antes de tentar remov-lo
            await ValidateMenuItemExistsAsync(id, companyId);

            // Remove o item do cardpio
            await _menuItemRepository.DeleteAsync(id, companyId);
        });
    }

    /// <summary>
    /// Valida os parmetros de entrada.
    /// </summary>
    /// <param name="id">ID do item do cardpio.</param>
    /// <param name="companyId">ID da empresa.</param>
    private void ValidateInputParameters(string id, string companyId)
    {
        if (string.IsNullOrEmpty(id))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID do item do cardpio obrigatrio.", new ValidationResult());

        if (string.IsNullOrEmpty(companyId))
            throw new Hephaestus.Application.Exceptions.ValidationException("ID da empresa obrigatrio.", new ValidationResult());
    }

    /// <summary>
    /// Verifica se o item do cardpio existe.
    /// </summary>
    /// <param name="id">ID do item do cardpio.</param>
    /// <param name="companyId">ID da empresa.</param>
    private async Task ValidateMenuItemExistsAsync(string id, string companyId)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id, companyId);
        EnsureEntityExists(menuItem, "MenuItem", id);
    }
}
