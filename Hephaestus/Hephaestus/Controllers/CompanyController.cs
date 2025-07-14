using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Company;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de perfis de empresas.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly IGetCompanyProfileUseCase _getCompanyProfileUseCase;
    private readonly ILogger<CompanyController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CompanyController"/>.
    /// </summary>
    /// <param name="getCompanyProfileUseCase">Caso de uso para obtenção de perfil da empresa.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public CompanyController(IGetCompanyProfileUseCase getCompanyProfileUseCase, ILogger<CompanyController> logger)
    {
        _getCompanyProfileUseCase = getCompanyProfileUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o perfil completo de uma empresa.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "name": "Restaurante Exemplo",
    ///   "email": "contato@restaurante.com",
    ///   "phoneNumber": "11987654321",
    ///   "isEnabled": true,
    ///   "feeType": "Percentage",
    ///   "feeValue": 5.0,
    ///   "state": "SP",
    ///   "city": "São Paulo",
    ///   "neighborhood": "Centro",
    ///   "street": "Rua das Flores",
    ///   "number": "123",
    ///   "latitude": -23.5505,
    ///   "longitude": -46.6333,
    ///   "slogan": "A melhor comida da cidade!",
    ///   "description": "Restaurante especializado em pratos tradicionais.",
    ///   "images": [
    ///     {
    ///       "id": "img1",
    ///       "url": "https://example.com/image1.jpg",
    ///       "description": "Fachada do restaurante"
    ///     }
    ///   ],
    ///   "operatingHours": [
    ///     {
    ///       "dayOfWeek": 1,
    ///       "openTime": "08:00",
    ///       "closeTime": "22:00",
    ///       "isOpen": true
    ///     }
    ///   ],
    ///   "socialMedia": [
    ///     {
    ///       "platform": "Instagram",
    ///       "url": "https://instagram.com/restaurante"
    ///     }
    ///   ]
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Empresa não encontrada."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da empresa.</param>
    /// <returns>Perfil completo da empresa.</returns>
    [HttpGet("{id}/profile")]
    [SwaggerOperation(Summary = "Obtém perfil da empresa", Description = "Retorna informações do perfil da empresa, incluindo imagens, horários e redes sociais.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyProfileResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCompanyProfile(string id)
    {
        var profile = await _getCompanyProfileUseCase.ExecuteAsync(id);
        return Ok(profile);
    }
}