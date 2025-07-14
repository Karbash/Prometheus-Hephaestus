using Hephaestus.Application.DTOs.Response;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface IGetCustomerOrderStatusUseCase
{
    Task<IEnumerable<OrderStatusResponse>> ExecuteAsync(string customerPhoneNumber, ClaimsPrincipal user);
}