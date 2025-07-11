using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Company;
using Hephaestus.Domain.Repositories;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Company;

public class GetCompanyProfileUseCase : IGetCompanyProfileUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICompanyImageRepository _companyImageRepository;
    private readonly ICompanyOperatingHourRepository _companyOperatingHourRepository;
    private readonly ICompanySocialMediaRepository _companySocialMediaRepository;

    public GetCompanyProfileUseCase(
        ICompanyRepository companyRepository,
        ICompanyImageRepository companyImageRepository,
        ICompanyOperatingHourRepository companyOperatingHourRepository,
        ICompanySocialMediaRepository companySocialMediaRepository)
    {
        _companyRepository = companyRepository;
        _companyImageRepository = companyImageRepository;
        _companyOperatingHourRepository = companyOperatingHourRepository;
        _companySocialMediaRepository = companySocialMediaRepository;
    }

    public async Task<CompanyProfileResponse> ExecuteAsync(string companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
            throw new KeyNotFoundException("Empresa não encontrada.");

        var images = await _companyImageRepository.GetByCompanyIdAsync(companyId);
        var operatingHours = await _companyOperatingHourRepository.GetByCompanyIdAsync(companyId);
        var socialMedia = await _companySocialMediaRepository.GetByCompanyIdAsync(companyId);

        return new CompanyProfileResponse
        {
            Id = company.Id,
            Name = company.Name,
            Email = company.Email,
            PhoneNumber = company.PhoneNumber,
            State = company.State,
            City = company.City,
            Street = company.Street,
            Number = company.Number,
            Latitude = company.Latitude,
            Longitude = company.Longitude,
            Slogan = company.Slogan,
            Description = company.Description,
            Images = images.Select(i => new CompanyImageResponse
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ImageType = i.ImageType
            }).ToList(),
            OperatingHours = operatingHours.Select(oh => new CompanyOperatingHourResponse
            {
                DayOfWeek = oh.DayOfWeek,
                OpenTime = oh.OpenTime,
                CloseTime = oh.CloseTime,
                IsClosed = oh.IsClosed
            }).ToList(),
            SocialMedia = socialMedia.Select(sm => new CompanySocialMediaResponse
            {
                Platform = sm.Platform,
                Url = sm.Url
            }).ToList()
        };
    }
}