using Hephaestus.Application.DTOs.Response;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Order;

public interface IGetOrderByIdUseCase
{
    Task<OrderResponse> ExecuteAsync(string id, string tenantId);
}