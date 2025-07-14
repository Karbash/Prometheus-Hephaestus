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

    /// <summary>
    /// Inicializa uma nova instância do <see cref="CustomerController"/>.
    /// </summary>
    /// <param name="updateCustomerUseCase">Caso de uso para atualização de clientes.</param>
    /// <param name="getCustomerUseCase">Caso de uso para listagem de clientes.</param>
    /// <param name="getByIdCustomerUseCase">Caso de uso para obtenção de cliente por ID.</param>
    /// <param name="auditLogUseCase">Caso de uso para logs de auditoria.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
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
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```
    /// Status: 204 No Content
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "TenantId não encontrado no token."
    /// }
    /// ```
    /// </remarks>
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
        var tenantId = GetTenantId();
        await _updateCustomerUseCase.UpdateAsync(request, tenantId);
        await _auditLogUseCase.ExecuteAsync("UpdateCustomer", request.PhoneNumber, $"Cliente {request.PhoneNumber} atualizado/cadastrado.", User);
        return NoContent();
    }

    /// <summary>
    /// Lista clientes do tenant.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "phoneNumber": "11987654321",
    ///     "name": "João Silva",
    ///     "state": "SP",
    ///     "city": "São Paulo",
    ///     "street": "Rua das Flores",
    ///     "number": "123",
    ///     "latitude": -23.5505,
    ///     "longitude": -46.6333,
    ///     "createdAt": "2024-01-01T12:00:00Z"
    ///   }
    /// ]
    /// ```
    /// </remarks>
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
        var tenantId = GetTenantId();
        var customers = await _getCustomerUseCase.GetAsync(phoneNumber, tenantId);
        return Ok(customers);
    }

    /// <summary>
    /// Obtém detalhes de um cliente específico.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "phoneNumber": "11987654321",
    ///   "name": "João Silva",
    ///   "state": "SP",
    ///   "city": "São Paulo",
    ///   "street": "Rua das Flores",
    ///   "number": "123",
    ///   "latitude": -23.5505,
    ///   "longitude": -46.6333,
    ///   "createdAt": "2024-01-01T12:00:00Z"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Cliente não encontrado."
    /// }
    /// ```
    /// </remarks>
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
        var tenantId = GetTenantId();
        var customer = await _getByIdCustomerUseCase.GetByIdAsync(id, tenantId);
        
        if (customer == null)
            return NotFound(new { error = "Cliente não encontrado." });

        return Ok(customer);
    }

    /// <summary>
    /// Obtém o TenantId do token de autenticação.
    /// </summary>
    /// <returns>TenantId do token.</returns>
    private string GetTenantId()
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("TenantId não encontrado no token para o usuário {UserId}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            throw new UnauthorizedAccessException("TenantId não encontrado no token.");
        }
        return tenantId;
    }
}