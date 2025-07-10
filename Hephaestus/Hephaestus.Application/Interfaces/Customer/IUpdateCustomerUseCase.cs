using Hephaestus.Application.DTOs.Request;

namespace Hephaestus.Application.Interfaces.Customer;

public interface IUpdateCustomerUseCase
{
    Task UpdateAsync(CustomerRequest request, string tenantId);
}