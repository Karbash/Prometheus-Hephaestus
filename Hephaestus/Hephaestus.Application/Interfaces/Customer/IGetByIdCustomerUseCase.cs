using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Customer;

public interface IGetByIdCustomerUseCase
{
    Task<CustomerResponse?> GetByIdAsync(string id, string tenantId);
}