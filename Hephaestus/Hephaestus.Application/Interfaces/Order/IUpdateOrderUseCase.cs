using Hephaestus.Application.DTOs.Request;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Order;

public interface IUpdateOrderUseCase
{
    Task ExecuteAsync(string id, UpdateOrderRequest request, string tenantId);
}