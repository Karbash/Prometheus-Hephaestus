using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Administration;

public class GetCompaniesUseCase : IGetCompaniesUseCase
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompaniesUseCase(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<CompanyResponse>> ExecuteAsync(bool? isEnabled)
    {
        var companies = await _companyRepository.GetAllAsync(isEnabled);

        return companies.Select(c => new CompanyResponse
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            IsEnabled = c.IsEnabled,
            FeeType = c.FeeType.ToString(),
            FeeValue = (double)c.FeeValue,
            State = c.State, // Novo campo
            City = c.City,
            Neighborhood = c.Neighborhood,
            Street = c.Street,
            Number = c.Number,
            Latitude = c.Latitude,
            Longitude = c.Longitude,
            Slogan = c.Slogan,
            Description = c.Description
        });
    }
}