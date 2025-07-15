using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Order
{
    public interface IPatchOrderUseCase
    {
        Task ExecuteAsync(UpdateOrderRequest request, ClaimsPrincipal user);
    }
} 