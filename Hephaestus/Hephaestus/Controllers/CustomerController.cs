using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Application.Interfaces.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims; // Necessário para ClaimTypes

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de clientes, permitindo operações como atualização, cadastro e consulta de clientes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Tenant")]
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
    /// <param name="updateCustomerUseCase">Caso de uso para atualizar ou cadastrar clientes.</param>
    /// <param name="getCustomerUseCase">Caso de uso para listar clientes.</param>
    /// <param name="getByIdCustomerUseCase">Caso de uso para obter um cliente por ID.</param>
    /// <param name="auditLogUseCase">Caso de uso para registrar logs de auditoria.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
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
    /// Atualiza ou cadastra um cliente com base no número de telefone.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um administrador ou um tenant atualize as informações de um cliente existente ou cadastre um novo cliente se o número de telefone não for encontrado.
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "phoneNumber": "11998877665",
    ///   "name": "Maria Oliveira",
    ///   "state": "MG",
    ///   "city": "Belo Horizonte",
    ///   "street": "Avenida Afonso Pena",
    ///   "number": "500",
    ///   "latitude": -19.9208,
    ///   "longitude": -43.9378
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 204 No Content):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 204)
    /// ```
    /// 
    /// Exemplo de erro de validação (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "PhoneNumber": [
    ///       "O número de telefone é inválido."
    ///     ]
    ///   }
    /// }
    /// ```
    /// Exemplo de erro de autenticação (Status 401 Unauthorized):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 401)
    /// ```
    /// </remarks>
    /// <param name="request">Dados do cliente a serem atualizados ou cadastrados.</param>
    /// <returns>Um `NoContentResult` indicando o sucesso da operação.</returns>
    [HttpPut]
    [SwaggerOperation(Summary = "Atualiza ou cadastra cliente", Description = "Atualiza as informações de um cliente existente ou cadastra um novo cliente com base no número de telefone. Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCustomer([FromBody] CustomerRequest request)
    {
        await _updateCustomerUseCase.UpdateAsync(request, User);
        await _auditLogUseCase.ExecuteAsync("UpdateCustomer", request.PhoneNumber, $"Cliente {request.PhoneNumber} atualizado/cadastrado.", User);
        return NoContent();
    }

    /// <summary>
    /// Lista clientes do tenant, com opções de filtro e paginação.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna uma lista paginada de clientes pertencentes ao tenant autenticado. É possível filtrar a lista por número de telefone.
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "123e4567-e89b-12d3-a456-426614174001",
    ///       "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "phoneNumber": "11987654321",
    ///       "name": "João Silva",
    ///       "state": "SP",
    ///       "city": "São Paulo",
    ///       "street": "Rua das Flores",
    ///       "number": "123",
    ///       "latitude": -23.5505,
    ///       "longitude": -46.6333,
    ///       "createdAt": "2025-07-14T12:00:00Z"
    ///     },
    ///     {
    ///       "id": "a9b8c7d6-e5f4-3g2h-1i0j-k9l8m7n6o5p4",
    ///       "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "phoneNumber": "21991122334",
    ///       "name": "Ana Souza",
    ///       "state": "RJ",
    ///       "city": "Rio de Janeiro",
    ///       "street": "Rua Copacabana",
    ///       "number": "456",
    ///       "latitude": -22.9710,
    ///       "longitude": -43.1820,
    ///       "createdAt": "2025-07-13T10:30:00Z"
    ///     }
    ///   ],
    ///   "totalCount": 2,
    ///   "pageNumber": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    /// Exemplo de erro de autenticação (Status 401 Unauthorized):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 401)
    /// ```
    /// </remarks>
    /// <param name="phoneNumber">Filtro opcional: número de telefone do cliente.</param>
    /// <param name="pageNumber">Número da página para paginação (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página para paginação (padrão: 20).</param>
    /// <returns>Um `OkResult` contendo uma lista paginada de `CustomerResponse`.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista clientes do tenant", Description = "Retorna uma lista paginada de clientes do tenant autenticado, com filtros opcionais. Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<CustomerResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Para paginação inválida, por exemplo
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? phoneNumber = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var customers = await _getCustomerUseCase.ExecuteAsync(phoneNumber, User, pageNumber, pageSize);
        return Ok(customers);
    }

    /// <summary>
    /// Obtém os detalhes de um cliente específico por seu ID.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todas as informações de um cliente, desde que o cliente pertença ao tenant autenticado.
    /// Requer autenticação com as roles **Admin** ou **Tenant**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
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
    ///   "createdAt": "2025-07-14T12:00:00Z"
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID do cliente 'abc-123' não é um GUID válido."
    /// }
    /// ```
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Cliente com ID '99999999-9999-9999-9999-999999999999' não encontrado para o tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do cliente a ser consultado.</param>
    /// <returns>Um `OkResult` contendo o `CustomerResponse` ou um `NotFoundResult` se o cliente não for encontrado.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém cliente por ID", Description = "Retorna detalhes de um cliente específico do tenant autenticado. Requer autenticação com Role=Admin ou Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Adicionado para ID inválido
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerById(string id)
    {
        var customer = await _getByIdCustomerUseCase.GetByIdAsync(id, User);
        if (customer == null)
            return NotFound(new { error = new { code = "NOT_FOUND", message = "Cliente não encontrado" } });
        return Ok(customer);
    }
}