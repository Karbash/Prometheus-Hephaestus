using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Interfaces.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de clientes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class CustomerController : ControllerBase
{
    private readonly IUpdateCustomerUseCase _updateCustomerUseCase;
    private readonly IGetCustomerUseCase _getCustomerUseCase;
    private readonly IGetByIdCustomerUseCase _getByIdCustomerUseCase;
    private readonly IAuditLogUseCase _auditLogUseCase;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(
        IUpdateCustomerUseCase updateCustomerUseCase,
        IGetCustomerUseCase getCustomerUseCase,
        IGetByIdCustomerUseCase getByIdCustomerUseCase,
        IAuditLogUseCase auditLogUseCase,
        ILogger<CustomerController> logger)
    {
        _updateCustomerUseCase = updateCustomerUseCase;
        _getCustomerUseCase = getCustomerUseCase;
        _getByIdCustomerUseCase = getByIdCustomerUseCase;
        _auditLogUseCase = auditLogUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Atualiza ou cadastra dados de um cliente.
    /// </summary>
    /// <param name="request">Dados do cliente (nome, endereço, latitude, longitude).</param>
    /// <returns>Status da atualização ou criação.</returns>
    [HttpPut]
    [SwaggerOperation(Summary = "Atualiza ou cadastra cliente", Description = "Atualiza os dados de um cliente existente ou cadastra um novo cliente com base no número de telefone. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> UpdateCustomer([FromBody] CustomerRequest request)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest(new { error = "TenantId não encontrado no token." });

            await _updateCustomerUseCase.UpdateAsync(request, tenantId);
            await _auditLogUseCase.ExecuteAsync("UpdateCustomer", request.PhoneNumber, $"Cliente {request.PhoneNumber} atualizado/cadastrado.", User);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar/cadastrar cliente com telefone {PhoneNumber}.", request.PhoneNumber);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar/cadastrar cliente com telefone {PhoneNumber}.", request.PhoneNumber);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista clientes do tenant.
    /// </summary>
    /// <param name="phoneNumber">Número de telefone para filtrar clientes (opcional).</param>
    /// <returns>Lista de clientes.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista clientes", Description = "Retorna uma lista de clientes do tenant, com filtro opcional por número de telefone. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CustomerResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCustomers([FromQuery] string? phoneNumber = null)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest(new { error = "TenantId não encontrado no token." });

            var customers = await _getCustomerUseCase.GetAsync(phoneNumber, tenantId);
            return Ok(customers);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao listar clientes.");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar clientes.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém detalhes de um cliente específico.
    /// </summary>
    /// <param name="id">ID do cliente (GUID).</param>
    /// <returns>Detalhes do cliente.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém detalhes de um cliente", Description = "Retorna os detalhes de um cliente com base no ID. Requer autenticação com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetCustomerById(string id)
    {
        try
        {
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest(new { error = "TenantId não encontrado no token." });

            var customer = await _getByIdCustomerUseCase.GetByIdAsync(id, tenantId);
            if (customer == null)
                return NotFound(new { error = "Cliente não encontrado." });

            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao obter cliente com ID {Id}.", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter cliente com ID {Id}.", id);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}