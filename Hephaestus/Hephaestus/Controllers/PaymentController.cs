using Hephaestus.Application.Interfaces.Payment;
using Hephaestus.Domain.DTOs.Request; // Assuming this is where PaymentRequest lives
using Hephaestus.Domain.DTOs.Response; // Assuming this is where PaymentResponse lives
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations; // Added for Swagger annotations
using System.Security.Claims;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller for processing payments for a specific tenant.
/// </summary>
[ApiController]
[Route("api/payment")]
[Authorize(Roles = "Tenant")]
public class PaymentController : ControllerBase
{
    private readonly IProcessPaymentUseCase _processPaymentUseCase;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentController"/>.
    /// </summary>
    /// <param name="processPaymentUseCase">The use case for processing payments.</param>
    public PaymentController(IProcessPaymentUseCase processPaymentUseCase)
    {
        _processPaymentUseCase = processPaymentUseCase;
    }

    /// ProcessPayment

    /// <summary>
    /// Processes a new payment for the authenticated tenant.
    /// </summary>
    /// <remarks>
    /// This endpoint allows a tenant to initiate and process a payment transaction.
    /// It requires authentication with the **Tenant** role.
    ///
    /// **Example Request:**
    /// ```json
    /// {
    ///   "orderId": "e4f5g6h7-i8j9-0k1l-2m3n-4o5p6q7r8s9t",
    ///   "amount": 120.50,
    ///   "currency": "BRL",
    ///   "paymentMethod": "CreditCard",
    ///   "cardDetails": {
    ///     "cardNumber": "4111222233334444",
    ///     "expirationMonth": "12",
    ///     "expirationYear": "2027",
    ///     "cvv": "123",
    ///     "cardHolderName": "JOAO DA SILVA"
    ///   },
    ///   "customerName": "João da Silva",
    ///   "customerEmail": "joao.silva@example.com"
    /// }
    /// ```
    ///
    /// **Example Success Response (Status 201 Created):**
    /// ```json
    /// {
    ///   "transactionId": "txn_abcdef1234567890",
    ///   "status": "Approved",
    ///   "amount": 120.50,
    ///   "currency": "BRL",
    ///   "orderId": "e4f5g6h7-i8j9-0k1l-2m3n-4o5p6q7r8s9t",
    ///   "timestamp": "2024-07-14T13:00:00Z",
    ///   "message": "Pagamento aprovado com sucesso."
    /// }
    /// ```
    ///
    /// **Example Validation Error (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Amount": [
    ///       "O valor do pagamento deve ser maior que zero."
    ///     ],
    ///     "CardDetails.CardNumber": [
    ///       "Número do cartão inválido."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Example Unauthorized (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token."
    /// }
    /// ```
    ///
    /// **Example Not Found (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Pedido com ID '99999999-9999-9999-9999-999999999999' não encontrado."
    /// }
    /// ```
    ///
    /// **Example Conflict (Status 409 Conflict):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "Este pedido já possui um pagamento registrado ou está em um status que impede novo pagamento."
    /// }
    /// ```
    ///
    /// **Example Internal Server Error (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao processar o pagamento."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">The payment request details (<see cref="PaymentRequest"/>).</param>
    /// <returns>A <see cref="StatusCodeResult"/> with status 201 Created and the <see cref="PaymentResponse"/>.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Processes a payment", Description = "Processes a new payment transaction for the authenticated tenant. Requires authentication with Role=Tenant.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PaymentResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Explicitly using ProblemDetails for 401
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        var response = await _processPaymentUseCase.ExecuteAsync(request, User);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}