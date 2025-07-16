using Hephaestus.Domain.DTOs.Request;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Customer;

public interface IUpdateCustomerUseCase
{
    Task UpdateAsync(CustomerRequest request, ClaimsPrincipal user);
}
