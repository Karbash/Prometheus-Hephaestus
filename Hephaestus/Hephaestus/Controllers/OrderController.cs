using Hephaestus.Domain.DTOs.Request;
// using Hephaestus.Domain.DTOs.Response; // Removido, pois as DTOs n�o foram fornecidas e a instru��o � n�o alterar m�todos
using Hephaestus.Application.Interfaces.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations; // Adicionado para as anota��es do Swagger
using System.Security.Claims;
using Hephaestus.Domain.DTOs.Response; // Necess�rio para ClaimTypes

namespace Hephaestus.Api.Controllers;

/// <summary>
/// Controller para gerenciamento de pedidos, permitindo a cria��o, consulta, atualiza��o e rastreamento de status de pedidos.
/// </summary>
[Route("api/order")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class OrderController : ControllerBase
{
    private readonly ICreateOrderUseCase _createOrderUseCase;
    private readonly IGetOrdersUseCase _getOrdersUseCase;
    private readonly IGetOrderByIdUseCase _getOrderByIdUseCase;
    private readonly IUpdateOrderUseCase _updateOrderUseCase;
    private readonly IGetCustomerOrderStatusUseCase _getCustomerOrderStatusUseCase;
    private readonly IPatchOrderUseCase _patchOrderUseCase;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="OrderController"/>.
    /// </summary>
    /// <param name="createOrderUseCase">Caso de uso para cria��o de pedidos.</param>
    /// <param name="getOrdersUseCase">Caso de uso para listagem de pedidos.</param>
    /// <param name="getOrderByIdUseCase">Caso de uso para obten��o de pedido por ID.</param>
    /// <param name="updateOrderUseCase">Caso de uso para atualiza��o de pedidos.</param>
    /// <param name="getCustomerOrderStatusUseCase">Caso de uso para obten��o do status de pedidos de um cliente.</param>
    public OrderController(
        ICreateOrderUseCase createOrderUseCase,
        IGetOrdersUseCase getOrdersUseCase,
        IGetOrderByIdUseCase getOrderByIdUseCase,
        IUpdateOrderUseCase updateOrderUseCase,
        IGetCustomerOrderStatusUseCase getCustomerOrderStatusUseCase,
        IPatchOrderUseCase patchOrderUseCase)
    {
        _createOrderUseCase = createOrderUseCase;
        _getOrdersUseCase = getOrdersUseCase;
        _getOrderByIdUseCase = getOrderByIdUseCase;
        _updateOrderUseCase = updateOrderUseCase;
        _getCustomerOrderStatusUseCase = getCustomerOrderStatusUseCase;
        _patchOrderUseCase = patchOrderUseCase;
    }

    /// CreateOrder

    /// <summary>
    /// Cria um novo pedido para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant registre um novo pedido. A autentica��o com a role **Tenant** � obrigat�ria.
    ///
    /// **Exemplo de Requisi��o:**
    /// ```json
    /// {
    ///   "customerPhoneNumber": "11987654321",
    ///   "items": [
    ///     {
    ///       "menuItemId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
    ///       "quantity": 2,
    ///       "customizations": [
    ///         {
    ///           "type": "Tamanho",
    ///           "value": "Grande"
    ///         },
    ///         {
    ///           "type": "Molho",
    ///           "value": "Barbecue"
    ///         }
    ///       ]
    ///     }
    ///   ],
    ///   "notes": "Entrega sem contato"
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 201 Created):**
    /// ```json
    /// {
    ///   "id": "e4f5g6h7-i8j9-0k1l-2m3n-4o5p6q7r8s9t"
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 401 Unauthorized):**
    /// ```
    /// (Sem corpo de resposta, apenas status 401)
    /// ```
    ///
    /// **Exemplo de Erro (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "CustomerPhoneNumber": [
    ///       "O n�mero de telefone do cliente � obrigat�rio."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao criar o pedido."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados para a cria��o do pedido (<see cref="CreateOrderRequest"/>).</param>
    /// <returns>Um <see cref="CreatedAtActionResult"/> contendo o ID do pedido criado.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo pedido", Description = "Cria um novo pedido para o tenant autenticado. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))] // O tipo � 'object' porque o retorno � um tipo an�nimo `{ Id = orderId }`
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var orderId = await _createOrderUseCase.ExecuteAsync(request, User);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { Id = orderId });
    }


    ///### GetOrders

    /// <summary>
    /// Lista pedidos para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite consultar uma lista de pedidos associados ao tenant autenticado.
    /// � poss�vel filtrar os resultados por n�mero de telefone do cliente e/ou pelo status do pedido.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// [
    ///   {
    ///     "id": "e4f5g6h7-i8j9-0k1l-2m3n-4o5p6q7r8s9t",
    ///     "customerPhoneNumber": "11987654321",
    ///     "status": "Pending",
    ///     "totalAmount": 51.80,
    ///     "createdAt": "2024-07-14T10:00:00Z"
    ///   },
    ///   {
    ///     "id": "a1b2c3d4-e5f6-7890-1234-567890fedcba",
    ///     "customerPhoneNumber": "11998877665",
    ///     "status": "Completed",
    ///     "totalAmount": 7.00,
    ///     "createdAt": "2024-07-13T15:30:00Z"
    ///   }
    /// ]
    /// ```
    ///
    /// **Exemplo de Erro (Status 401 Unauthorized):**
    /// ```
    /// (Sem corpo de resposta, apenas status 401)
    /// ```
    ///
    /// **Exemplo de Erro (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro ao buscar os pedidos."
    /// }
    /// ```
    /// </remarks>
    /// <param name="customerPhoneNumber">Filtro opcional: n�mero de telefone do cliente.</param>
    /// <param name="status">Filtro opcional: status do pedido (e.g., 'Pending', 'Processing', 'Completed', 'Cancelled').</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo uma lista de pedidos.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista pedidos", Description = "Retorna uma lista de pedidos do tenant, com filtros opcionais. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<OrderResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? customerPhoneNumber,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _getOrdersUseCase.ExecuteAsync(User, customerPhoneNumber, status, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// GetOrderById

    /// <summary>
    /// Obt�m os detalhes de um pedido espec�fico por ID.
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna todas as informa��es de um pedido, desde que o pedido perten�a ao tenant autenticado.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// {
    ///   "id": "e4f5g6h7-i8j9-0k1l-2m3n-4o5p6q7r8s9t",
    ///   "customerPhoneNumber": "11987654321",
    ///   "deliveryAddress": {
    ///     "street": "Rua das Flores",
    ///     "number": "123"
    ///   },
    ///   "status": "Processing",
    ///   "totalAmount": 65.10
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 401 Unauthorized):**
    /// ```
    /// (Sem corpo de resposta, apenas status 401)
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Pedido com ID '99999999-9999-9999-9999-999999999999' n�o encontrado para este tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro ao buscar o pedido."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do pedido a ser consultado.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo os detalhes do pedido, ou um <see cref="NotFoundResult"/> se n�o encontrado.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obt�m pedido por ID", Description = "Retorna os detalhes de um pedido espec�fico do tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))] // Usando object, pois o DTO OrderResponse n�o foi fornecido
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetOrderById(string id)
    {
        var order = await _getOrderByIdUseCase.ExecuteAsync(id, User);
        return Ok(order);
    }

    /// UpdateOrder

    /// <summary>
    /// Atualiza um pedido existente para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite atualizar um pedido existente do tenant. A autentica��o com a role **Tenant** � obrigat�ria.
    ///
    /// **Exemplo de Requisi��o:**
    /// ```json
    /// {
    ///   "status": "Completed",
    ///   "deliveryNotes": "Pedido entregue ao cliente."
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 204 No Content):**
    /// ```
    /// (Sem corpo de resposta, apenas status 204)
    /// ```
    ///
    /// **Exemplo de Erro (Status 401 Unauthorized):**
    /// ```
    /// (Sem corpo de resposta, apenas status 401)
    /// ```
    ///
    /// **Exemplo de Erro (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Status": [
    ///       "O status fornecido � inv�lido."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Pedido com ID '99999999-9999-9999-9999-999999999999' n�o encontrado para atualiza��o."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao atualizar o pedido."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">O **ID (GUID)** do pedido a ser atualizado.</param>
    /// <param name="request">Dados para a atualiza��o do pedido (<see cref="UpdateOrderRequest"/>).</param>
    /// <returns>Um <see cref="NoContentResult"/> indicando o sucesso da atualiza��o.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza um pedido", Description = "Atualiza os detalhes de um pedido existente para o tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderRequest request)
    {
        await _updateOrderUseCase.ExecuteAsync(request, User);
        return NoContent();
    }

    /// <summary>
    /// Atualiza parcialmente um pedido existente para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Envie apenas os campos que deseja alterar. Campos n�o enviados permanecem inalterados.
    /// </remarks>
    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Atualiza parcialmente um pedido", Description = "Atualiza apenas os campos enviados do pedido para o tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> PatchOrder(string id, [FromBody] UpdateOrderRequest request)
    {
        request.OrderId = id;
        await _patchOrderUseCase.ExecuteAsync(request, User);
        return NoContent();
    }

    /// <summary>
    /// Atualiza o status de um pedido.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <param name="request">Novo status do pedido.</param>
    /// <returns>Pedido atualizado.</returns>
    /// <response code="200">Status atualizado com sucesso.</response>
    /// <response code="400">Transi��o de status inv�lida.</response>
    /// <response code="404">Pedido n�o encontrado.</response>
    [HttpPatch("{id}/status")]
    [SwaggerOperation(Summary = "Atualizar status do pedido", Description = "Atualiza o status de um pedido. S� permite transi��es v�lidas.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _getOrderByIdUseCase.ExecuteAsync(id, User);
        if (order == null)
            return NotFound();

        var currentStatus = order.Status.ToString();
        var newStatus = request.Status.ToString();
        // Corrigir para evitar ?. em enum n�o-nullable
        var paymentStatus = order.PaymentStatus.ToString();

        // Regras de transi��o
        if (currentStatus == "Pending")
        {
            if (newStatus == "Cancelled")
            {
                if (paymentStatus == "Paid")
                    return BadRequest("N�o � poss�vel cancelar um pedido j� pago.");
            }
            else if (newStatus == "Completed")
            {
                if (paymentStatus != "Paid")
                    return BadRequest("S� � permitido finalizar um pedido j� pago.");
            }
            else if (newStatus != "InProduction")
            {
                return BadRequest("Transi��o de status inv�lida para pedido pendente.");
            }
        }
        else if (currentStatus == "InProduction")
        {
            if (newStatus == "Cancelled")
            {
                if (paymentStatus == "Paid")
                    return BadRequest("N�o � poss�vel cancelar um pedido j� pago.");
            }
            else if (newStatus == "Completed")
            {
                if (paymentStatus != "Paid")
                    return BadRequest("S� � permitido finalizar um pedido j� pago.");
            }
            else if (newStatus != "Pending")
            {
                return BadRequest("Transi��o de status inv�lida para pedido em produ��o.");
            }
        }
        else if (currentStatus == "Completed" || currentStatus == "Cancelled")
        {
            return BadRequest("N�o � permitido alterar o status de um pedido j� finalizado ou cancelado.");
        }

        // Atualiza status
        var updateRequest = new UpdateOrderRequest
        {
            OrderId = order.Id,
            Status = request.Status
        };
        await _updateOrderUseCase.ExecuteAsync(updateRequest, User);
        // Retorna o pedido atualizado
        var updated = await _getOrderByIdUseCase.ExecuteAsync(id, User);
        return Ok(updated);
    }

    /// <summary>
    /// Atualiza o status de pagamento de um pedido.
    /// </summary>
    /// <param name="id">ID do pedido.</param>
    /// <param name="request">Novo status de pagamento.</param>
    /// <returns>Pedido atualizado.</returns>
    /// <response code="200">Status de pagamento atualizado com sucesso.</response>
    /// <response code="400">Transi��o de status inv�lida.</response>
    /// <response code="404">Pedido n�o encontrado.</response>
    [HttpPatch("{id}/payment-status")]
    [SwaggerOperation(Summary = "Atualizar status de pagamento do pedido", Description = "Atualiza o status de pagamento de um pedido. S� permite transi��es v�lidas. Ao aprovar pagamento, o status do pedido muda automaticamente para InProduction.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderPaymentStatus(string id, [FromBody] UpdateOrderPaymentStatusRequest request)
    {
        var patchRequest = new UpdateOrderRequest { OrderId = id, PaymentStatus = request.Status };
        // Regra: se status for Paid, tamb�m mover pedido para InProduction
        if (request.Status == Hephaestus.Domain.Enum.PaymentStatus.Paid)
            patchRequest.Status = Hephaestus.Domain.Enum.OrderStatus.InProduction;
        await _patchOrderUseCase.ExecuteAsync(patchRequest, User);
        return Ok();
    }

    ///### GetCustomerOrderStatus

    /// <summary>
    /// Obt�m o status dos pedidos de um cliente espec�fico.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um tenant consulte o status atual de todos os pedidos associados a um determinado n�mero de telefone de cliente.
    /// Requer autentica��o com a role **Tenant**.
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// [
    ///   {
    ///     "orderId": "e4f5g6h7-i8j9-0k1l-2m3n-4o5p6q7r8s9t",
    ///     "status": "Processing",
    ///     "lastUpdate": "2024-07-14T10:30:00Z"
    ///   },
    ///   {
    ///     "orderId": "f1e2d3c4-b5a6-9876-5432-10fedcba9876",
    ///     "status": "Pending",
    ///     "lastUpdate": "2024-07-14T11:00:00Z"
    ///   }
    /// ]
    /// ```
    ///
    /// **Exemplo de Erro (Status 401 Unauthorized):**
    /// ```
    /// (Sem corpo de resposta, apenas status 401)
    /// ```
    ///
    /// **Exemplo de Erro (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "CustomerPhoneNumber": [
    ///       "O n�mero de telefone do cliente � inv�lido."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro ao obter o status dos pedidos do cliente."
    /// }
    /// ```
    /// </remarks>
    /// <param name="customerPhoneNumber">O n�mero de telefone do cliente para o qual consultar o status dos pedidos.</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo uma lista com o status dos pedidos do cliente.</returns>
    [HttpGet("customer/status")]
    [SwaggerOperation(Summary = "Obt�m status de pedidos do cliente", Description = "Retorna o status atual dos pedidos de um cliente espec�fico para o tenant. Requer autentica��o com Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<object>))] // Usando object, pois o DTO n�o foi fornecido
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetCustomerOrderStatus([FromQuery] string customerPhoneNumber)
    {
        var statuses = await _getCustomerOrderStatusUseCase.ExecuteAsync(customerPhoneNumber, User);
        return Ok(statuses);
    }
}
