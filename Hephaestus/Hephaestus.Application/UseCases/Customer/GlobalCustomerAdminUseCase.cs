using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using System.Linq;

namespace Hephaestus.Application.UseCases.Customer;

public class GlobalCustomerAdminUseCase : BaseUseCase, IGlobalCustomerAdminUseCase
{
    private readonly ICustomerRepository _customerRepository;

    public GlobalCustomerAdminUseCase(
        ICustomerRepository customerRepository,
        ILogger<GlobalCustomerAdminUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResult<CustomerResponse>> ExecuteAsync(
        string? companyId = null,
        string? name = null,
        string? phoneNumber = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        string? sortOrder = "asc")
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var pagedCustomers = await _customerRepository.GetAllGlobalAsync(companyId, name, phoneNumber, isActive, pageNumber, pageSize, sortBy, sortOrder);
            return new PagedResult<CustomerResponse>
            {
                Items = pagedCustomers.Items.Select(c => new CustomerResponse
                {
                    Id = c.Id,
                    CompanyId = null, // Valor padrão, pois não existe na entidade
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToList(),
                TotalCount = pagedCustomers.TotalCount,
                PageNumber = pagedCustomers.PageNumber,
                PageSize = pagedCustomers.PageSize
            };
        });
    }
} 