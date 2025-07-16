using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Customer;

public interface IGetByIdCustomerUseCase
{
    Task<CustomerResponse?> GetByIdAsync(string id, ClaimsPrincipal user);
}
