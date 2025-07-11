using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Tag;
using Hephaestus.Domain.Repositories;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Tag;

public class GetAllTagsByTenantUseCase : IGetAllTagsByTenantUseCase
{
    private readonly ITagRepository _tagRepository;
    private readonly ICompanyRepository _companyRepository;

    public GetAllTagsByTenantUseCase(ITagRepository tagRepository, ICompanyRepository companyRepository)
    {
        _tagRepository = tagRepository;
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<TagResponse>> ExecuteAsync(string tenantId, ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
            throw new InvalidOperationException("Apenas administradores ou tenants podem listar tags.");

        var userTenantId = user.FindFirst("TenantId")?.Value;
        if (userRole == "Tenant" && userTenantId != tenantId)
            throw new InvalidOperationException("Tenants só podem listar suas próprias tags.");

        var company = await _companyRepository.GetByIdAsync(tenantId);
        if (company == null || company.Role.ToString() != "Tenant")
            throw new InvalidOperationException("Tenant inválido.");

        var tags = await _tagRepository.GetByTenantIdAsync(tenantId);
        return tags.Select(t => new TagResponse
        {
            Id = t.Id,
            TenantId = t.TenantId,
            Name = t.Name
        });
    }
}