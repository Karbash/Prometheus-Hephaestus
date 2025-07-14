using Hephaestus.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Order;

public interface IGetOrdersUseCase
{
    Task<IEnumerable<OrderResponse>> ExecuteAsync(string tenantId, string? customerPhoneNumber, string? status);
}