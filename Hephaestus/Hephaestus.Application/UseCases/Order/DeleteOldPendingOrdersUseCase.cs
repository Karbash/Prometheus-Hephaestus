using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Order;

/// <summary>
/// Use case para exclusão automática de pedidos Pending não pagos após X minutos.
/// </summary>
public class DeleteOldPendingOrdersUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<DeleteOldPendingOrdersUseCase> _logger;

    public DeleteOldPendingOrdersUseCase(IOrderRepository orderRepository, ILogger<DeleteOldPendingOrdersUseCase> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    /// <summary>
    /// Exclui pedidos Pending criados há mais de X minutos.
    /// </summary>
    /// <param name="minutes">Quantidade de minutos para considerar o pedido antigo.</param>
    public async Task ExecuteAsync(int minutes)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-minutes);
        var oldPendings = await _orderRepository.GetPendingOrdersOlderThanAsync(cutoff);
        foreach (var order in oldPendings)
        {
            await _orderRepository.DeleteAsync(order.Id, order.TenantId);
            _logger.LogInformation($"Pedido Pending {order.Id} excluído automaticamente.");
        }
    }
} 