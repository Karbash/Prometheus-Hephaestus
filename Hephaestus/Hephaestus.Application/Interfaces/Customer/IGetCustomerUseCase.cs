using Hephaestus.Application.DTOs.Response;

namespace Hephaestus.Application.Interfaces.Customer;

public interface IGetCustomerUseCase
{
    Task<IEnumerable<CustomerResponse>> GetAsync(string? phoneNumber, string tenantId);
}