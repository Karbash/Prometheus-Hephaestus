using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Order;

public interface ICreateOrderUseCase
{
    Task<string> ExecuteAsync(CreateOrderRequest request, ClaimsPrincipal user);
}
