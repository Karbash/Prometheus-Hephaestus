using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Company;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de perfis de empresas, fornecendo endpoints para consulta de dados da empresa.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly IGetCompanyProfileUseCase _getCompanyProfileUseCase;
    private readonly ILogger<CompanyController> _logger;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="CompanyController"/>.
    /// </summary>
    /// <param name="getCompanyProfileUseCase">Caso de uso para obter o perfil da empresa.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
    public CompanyController(IGetCompanyProfileUseCase getCompanyProfileUseCase, ILogger<CompanyController> logger)
    {
        _getCompanyProfileUseCase = getCompanyProfileUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Obt�m o perfil completo de uma empresa espec�fica.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todas as informa��es detalhadas de uma empresa, incluindo seus dados de contato, endere�o, slogan, descri��o, imagens, hor�rios de funcionamento e links de m�dias sociais.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
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
    ///   "city": "S�o Paulo",
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
    ///       "url": "[https://example.com/image1.jpg](https://example.com/image1.jpg)",
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
    ///       "url": "[https://instagram.com/restaurante](https://instagram.com/restaurante)"
    ///     }
    ///   ]
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Empresa com ID '99999999-9999-9999-9999-999999999999' n�o encontrada."
    /// }
    /// ```
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID fornecido 'abc-123' n�o � um GUID v�lido."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** da empresa a ser consultada.</param>
    /// <returns>Um `OkResult` contendo o `CompanyProfileResponse` ou um `NotFoundResult` se a empresa n�o for encontrada.</returns>
    [HttpGet("{id}/profile")]
    [SwaggerOperation(Summary = "Obt�m o perfil completo de uma empresa", Description = "Retorna todas as informa��es detalhadas do perfil de uma empresa, incluindo seus dados de contato, endere�o, m�dias sociais e mais.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyProfileResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Adicionado para ID inv�lido
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))] // Tipo de erro mais espec�fico
    [ProducesResponseType(StatusCodes.Status500InternalServerError)] // Removido Type = typeof(object) para padr�o ProblemDetails
    public async Task<IActionResult> GetCompanyProfile(string id)
    {
        var profile = await _getCompanyProfileUseCase.ExecuteAsync(id);
        return Ok(profile);
    }
}
