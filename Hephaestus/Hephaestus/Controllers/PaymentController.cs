using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controlador responsável pelo gerenciamento de pagamentos, reembolsos e comprovantes.
/// </summary>
[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Tenant")]
public class PaymentController : ControllerBase
{
    // Mock store para exemplo (substitua por serviço/repositório real)
    private static readonly Dictionary<string, PaymentResponse> Payments = new();
    private static readonly Dictionary<string, RefundResponse> Refunds = new();

    /// <summary>
    /// Cria uma nova transação de pagamento.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// {
    ///   "amount": 100.00,
    ///   "currency": "BRL",
    ///   "paymentMethod": "pix",
    ///   "customerId": "cliente123",
    ///   "orderId": "abc123"
    /// }
    /// </remarks>
    /// <param name="request">Dados do pagamento.</param>
    /// <returns>Dados do pagamento criado.</returns>
    /// <response code="201">Pagamento criado com sucesso.</response>
    [HttpPost]
    [SwaggerOperation(Summary = "Criar Pagamento", Description = "Cria uma nova transação de pagamento.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PaymentResponse))]
    public IActionResult CreatePayment([FromBody] PaymentRequest request)
    {
        var paymentId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;
        var response = new PaymentResponse
        {
            PaymentId = paymentId,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = "pending",
            PaymentMethod = request.PaymentMethod,
            CustomerId = request.CustomerId,
            OrderId = request.OrderId,
            CreatedAt = now,
            UpdatedAt = now
        };
        Payments[paymentId] = response;
        return CreatedAtAction(nameof(GetPayment), new { payment_id = paymentId }, response);
    }

    /// <summary>
    /// Consulta o status e detalhes de uma transação de pagamento.
    /// </summary>
    /// <param name="payment_id">ID do pagamento.</param>
    /// <returns>Dados do pagamento.</returns>
    /// <response code="200">Pagamento encontrado.</response>
    /// <response code="404">Pagamento não encontrado.</response>
    [HttpGet("{payment_id}")]
    [SwaggerOperation(Summary = "Consultar Pagamento", Description = "Consulta o status e detalhes de uma transação de pagamento.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPayment([FromRoute] string payment_id)
    {
        if (!Payments.TryGetValue(payment_id, out var payment))
            return NotFound();
        return Ok(payment);
    }

    /// <summary>
    /// Solicita o estorno total ou parcial de um pagamento.
    /// </summary>
    /// <param name="payment_id">ID do pagamento.</param>
    /// <param name="request">Dados do reembolso (valor opcional).</param>
    /// <returns>Dados do reembolso.</returns>
    /// <response code="200">Reembolso solicitado com sucesso.</response>
    /// <response code="404">Pagamento não encontrado.</response>
    [HttpPost("{payment_id}/refund")]
    [SwaggerOperation(Summary = "Reembolso", Description = "Solicita o estorno total ou parcial de um pagamento.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RefundResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RefundPayment([FromRoute] string payment_id, [FromBody] RefundRequest request)
    {
        if (!Payments.TryGetValue(payment_id, out var payment))
            return NotFound();
        var refundId = Guid.NewGuid().ToString();
        var amount = request.Amount ?? payment.Amount;
        var response = new RefundResponse
        {
            RefundId = refundId,
            PaymentId = payment_id,
            Amount = amount,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };
        Refunds[refundId] = response;
        // Atualiza status do pagamento para "refunded" se for reembolso total
        if (amount == payment.Amount)
            payment.Status = "refunded";
        Payments[payment_id] = payment;
        return Ok(response);
    }

    /// <summary>
    /// Atualiza o status de um pagamento.
    /// </summary>
    /// <param name="payment_id">ID do pagamento.</param>
    /// <param name="request">Novo status do pagamento.</param>
    /// <returns>Pagamento atualizado.</returns>
    /// <response code="200">Status atualizado com sucesso.</response>
    /// <response code="400">Transição de status inválida.</response>
    /// <response code="404">Pagamento não encontrado.</response>
    [HttpPatch("{payment_id}/status")]
    [SwaggerOperation(Summary = "Atualizar status do pagamento", Description = "Atualiza o status de um pagamento. Só permite transições válidas.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdatePaymentStatus([FromRoute] string payment_id, [FromBody] UpdatePaymentStatusRequest request)
    {
        if (!Payments.TryGetValue(payment_id, out var payment))
            return NotFound();

        var currentStatus = payment.Status.ToLower();
        var newStatus = request.Status.ToString().ToLower();

        // Regras de transição
        if (currentStatus == "pending")
        {
            if (newStatus != "paid" && newStatus != "failed" && newStatus != "refunded" && newStatus != "processed")
                return BadRequest("Transição de status inválida para pagamento pendente.");
        }
        else if (currentStatus == "paid")
        {
            if (newStatus != "refunded" && newStatus != "processed")
                return BadRequest("Só é permitido reembolsar ou processar um pagamento já pago.");
        }
        else if (currentStatus == "failed" || currentStatus == "refunded")
        {
            return BadRequest("Não é permitido alterar o status de um pagamento já falhado ou reembolsado.");
        }
        // processed pode ser terminal ou permitir outras regras, ajuste conforme necessário

        payment.Status = newStatus;
        payment.UpdatedAt = DateTime.UtcNow;
        Payments[payment_id] = payment;
        return Ok(payment);
    }

    /// <summary>
    /// Obtém o comprovante de pagamento (PDF ou JSON).
    /// </summary>
    /// <param name="payment_id">ID do pagamento.</param>
    /// <returns>URL ou dados do comprovante.</returns>
    /// <response code="200">Comprovante gerado com sucesso.</response>
    /// <response code="404">Pagamento não encontrado.</response>
    [HttpGet("{payment_id}/receipt")]
    [SwaggerOperation(Summary = "Gerar Comprovante", Description = "Obtém o comprovante de pagamento.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReceiptResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetReceipt([FromRoute] string payment_id)
    {
        if (!Payments.ContainsKey(payment_id))
            return NotFound();
        // Exemplo: retorna uma URL mock
        var response = new ReceiptResponse
        {
            ReceiptUrl = $"https://example.com/receipts/{payment_id}.pdf"
        };
        return Ok(response);
    }
}