using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Tenant")]
public class TagController : ControllerBase
{
    private readonly ICreateTagUseCase _createTagUseCase;
    private readonly IGetAllTagsByTenantUseCase _getAllTagsByTenantUseCase;
    private readonly IDeleteTagUseCase _deleteTagUseCase;
    private readonly ILogger<TagController> _logger;

    public TagController(
        ICreateTagUseCase createTagUseCase,
        IGetAllTagsByTenantUseCase getAllTagsByTenantUseCase,
        IDeleteTagUseCase deleteTagUseCase,
        ILogger<TagController> logger)
    {
        _createTagUseCase = createTagUseCase;
        _getAllTagsByTenantUseCase = getAllTagsByTenantUseCase;
        _deleteTagUseCase = deleteTagUseCase;
        _logger = logger;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Cria uma nova tag", Description = "Cria uma nova tag para o tenant autenticado. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest request)
    {
        try
        {
            var tagResponse = await _createTagUseCase.ExecuteAsync(request, User);
            return Ok(tagResponse);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao criar tag {Name}", request?.Name);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar tag {Name}", request?.Name);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lista todas as tags de um tenant", Description = "Retorna todas as tags associadas a um tenant. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TagResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetAllTagsByTenant([FromQuery] string tenantId)
    {
        try
        {
            var tags = await _getAllTagsByTenantUseCase.ExecuteAsync(tenantId, User);
            return Ok(tags);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao listar tags para tenant {TenantId}", tenantId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao listar tags para tenant {TenantId}", tenantId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Exclui uma tag", Description = "Exclui uma tag pelo ID, desde que não esteja associada a itens do cardápio. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> DeleteTag(string id)
    {
        try
        {
            await _deleteTagUseCase.ExecuteAsync(id, User);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao excluir tag {Id}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao excluir tag {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}