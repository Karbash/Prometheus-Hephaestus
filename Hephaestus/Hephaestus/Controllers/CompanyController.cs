using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Company;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly IGetCompanyProfileUseCase _getCompanyProfileUseCase;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(IGetCompanyProfileUseCase getCompanyProfileUseCase, ILogger<CompanyController> logger)
    {
        _getCompanyProfileUseCase = getCompanyProfileUseCase;
        _logger = logger;
    }

    [HttpGet("{id}/profile")]
    [SwaggerOperation(Summary = "Obtém perfil da empresa", Description = "Retorna informações do perfil da empresa, incluindo imagens, horários e redes sociais.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyProfileResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCompanyProfile(string id)
    {
        try
        {
            var profile = await _getCompanyProfileUseCase.ExecuteAsync(id);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Empresa {Id} não encontrada.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter perfil da empresa {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}