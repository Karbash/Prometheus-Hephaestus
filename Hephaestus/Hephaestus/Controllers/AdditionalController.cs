using FluentValidation;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Additional;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class AdditionalController : ControllerBase
{
    private readonly ICreateAdditionalUseCase _createAdditionalUseCase;
    private readonly IGetAdditionalsUseCase _getAdditionalsUseCase;
    private readonly IGetAdditionalByIdUseCase _getAdditionalByIdUseCase;
    private readonly IUpdateAdditionalUseCase _updateAdditionalUseCase;
    private readonly IDeleteAdditionalUseCase _deleteAdditionalUseCase;
    private readonly ILogger<AdditionalController> _logger;

    public AdditionalController(
        ICreateAdditionalUseCase createAdditionalUseCase,
        IGetAdditionalsUseCase getAdditionalsUseCase,
        IGetAdditionalByIdUseCase getAdditionalByIdUseCase,
        IUpdateAdditionalUseCase updateAdditionalUseCase,
        IDeleteAdditionalUseCase deleteAdditionalUseCase,
        ILogger<AdditionalController> logger)
    {
        _createAdditionalUseCase = createAdditionalUseCase;
        _getAdditionalsUseCase = getAdditionalsUseCase;
        _getAdditionalByIdUseCase = getAdditionalByIdUseCase;
        _updateAdditionalUseCase = updateAdditionalUseCase;
        _deleteAdditionalUseCase = deleteAdditionalUseCase;
        _logger = logger;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Cria adicional", Description = "Cria um novo adicional para o tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreateAdditional([FromBody] CreateAdditionalRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var id = await _createAdditionalUseCase.ExecuteAsync(request, tenantId);
            return CreatedAtAction(nameof(GetAdditionalById), new { id }, new { id });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar adicional.");
            return BadRequest(new { errors = ex.Errors });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao criar adicional.");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar adicional.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lista adicionais do tenant", Description = "Retorna a lista de adicionais do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AdditionalResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetAdditionals()
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var additionals = await _getAdditionalsUseCase.ExecuteAsync(tenantId);
            return Ok(additionals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar adicionais.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém adicional por ID", Description = "Retorna detalhes de um adicional. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdditionalResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetAdditionalById(string id)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            var additional = await _getAdditionalByIdUseCase.ExecuteAsync(id, tenantId);
            return Ok(additional);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Adicional {Id} não encontrado.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter adicional {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza adicional", Description = "Atualiza um adicional do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateAdditional(string id, [FromBody] UpdateAdditionalRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            await _updateAdditionalUseCase.ExecuteAsync(id, request, tenantId);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao atualizar adicional {Id}.", id);
            return BadRequest(new { errors = ex.Errors });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar adicional {Id}: {Message}.", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Adicional {Id} não encontrado.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar adicional {Id}: {Message}.", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar adicional {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove adicional", Description = "Remove um adicional do tenant. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeleteAdditional(string id)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return Unauthorized(new { error = "TenantId não encontrado no token." });

            await _deleteAdditionalUseCase.ExecuteAsync(id, tenantId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Adicional {Id} não encontrado.", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover adicional {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}