using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Api.Controllers;

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

    public OrderController(
        ICreateOrderUseCase createOrderUseCase,
        IGetOrdersUseCase getOrdersUseCase,
        IGetOrderByIdUseCase getOrderByIdUseCase,
        IUpdateOrderUseCase updateOrderUseCase,
        IGetCustomerOrderStatusUseCase getCustomerOrderStatusUseCase)
    {
        _createOrderUseCase = createOrderUseCase;
        _getOrdersUseCase = getOrdersUseCase;
        _getOrderByIdUseCase = getOrderByIdUseCase;
        _updateOrderUseCase = updateOrderUseCase;
        _getCustomerOrderStatusUseCase = getCustomerOrderStatusUseCase;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized();
        }

        var orderId = await _createOrderUseCase.ExecuteAsync(request, tenantId);
        return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, new { Id = orderId });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? customerPhoneNumber, [FromQuery] string? status)
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized();
        }

        var orders = await _getOrdersUseCase.ExecuteAsync(tenantId, customerPhoneNumber, status);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(string id)
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized();
        }

        var order = await _getOrderByIdUseCase.ExecuteAsync(id, tenantId);
        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderRequest request)
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized();
        }

        await _updateOrderUseCase.ExecuteAsync(id, request, tenantId);
        return NoContent();
    }

    [HttpGet("customer/status")]
    public async Task<IActionResult> GetCustomerOrderStatus([FromQuery] string customerPhoneNumber)
    {
        var tenantId = User.FindFirst("TenantId")?.Value;
        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized();
        }

        var statuses = await _getCustomerOrderStatusUseCase.ExecuteAsync(customerPhoneNumber, tenantId);
        return Ok(statuses);
    }
}