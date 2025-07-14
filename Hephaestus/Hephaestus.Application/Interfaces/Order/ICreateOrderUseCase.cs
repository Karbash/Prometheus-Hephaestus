using Hephaestus.Application.DTOs.Request;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Order;

public interface ICreateOrderUseCase
{
    Task<string> ExecuteAsync(CreateOrderRequest request, string tenantId);
}