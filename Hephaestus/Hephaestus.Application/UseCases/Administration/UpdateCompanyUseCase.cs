using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Application.Interfaces.Administration;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using System.Security.Claims;
using Hephaestus.Domain.Enum;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Administration;

/// <summary>
/// Caso de uso para atualização de empresas.
/// </summary>
public class UpdateCompanyUseCase : BaseUseCase, IUpdateCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="UpdateCompanyUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public UpdateCompanyUseCase(
        ICompanyRepository companyRepository, 
        IAuditLogRepository auditLogRepository,
        ILogger<UpdateCompanyUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
    }

    /// <summary>
    /// Executa a atualização de uma empresa.
    /// </summary>
    /// <param name="id">ID da empresa.</param>
    /// <param name="request">Dados atualizados da empresa.</param>
    /// <param name="user">Usuário autenticado.</param>
    public async Task ExecuteAsync(string id, UpdateCompanyRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            // Remover todas as linhas que usam _validator

            // Validação de autorização
            ValidateAuthorization(user);

            // Busca e validação da empresa
            var company = await GetAndValidateCompanyAsync(id);

            // Validação das regras de negócio
            await ValidateBusinessRulesAsync(request, id);

            // Atualização da empresa
            await UpdateCompanyAsync(company, request);

            // Registro de auditoria
            await CreateAuditLogAsync(company, user);
        }, "Atualização de Empresa");
    }

    /// <summary>
    /// Valida a autorização do usuário.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    private void ValidateAuthorization(ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        ValidateAuthorization(userRole == "Admin", "Apenas administradores podem atualizar empresas.", "Atualizar", "Empresa");
    }

    /// <summary>
    /// Busca e valida a empresa.
    /// </summary>
    /// <param name="id">ID da empresa.</param>
    /// <returns>Empresa encontrada.</returns>
    private async Task<Domain.Entities.Company> GetAndValidateCompanyAsync(string id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        EnsureEntityExists(company, "Empresa", id);
        return company!; // Garantido que não é null após EnsureEntityExists
    }

    /// <summary>
    /// Valida as regras de negócio.
    /// </summary>
    /// <param name="request">Requisição com os dados.</param>
    /// <param name="id">ID da empresa.</param>
    private async Task ValidateBusinessRulesAsync(UpdateCompanyRequest request, string id)
    {
        var existingByEmail = await _companyRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null && existingByEmail.Id != id)
            throw new ConflictException("E-mail já registrado.", "Empresa", "Email", request.Email);

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            var existingByPhone = await _companyRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingByPhone != null && existingByPhone.Id != id)
                throw new ConflictException("Telefone já registrado.", "Empresa", "PhoneNumber", request.PhoneNumber);
        }
    }

    /// <summary>
    /// Atualiza a empresa com os novos dados.
    /// </summary>
    /// <param name="company">Empresa a ser atualizada.</param>
    /// <param name="request">Dados atualizados.</param>
    private async Task UpdateCompanyAsync(Domain.Entities.Company company, UpdateCompanyRequest request)
    {
        company.Name = request.Name;
        company.Email = request.Email;
        company.PhoneNumber = request.PhoneNumber;
        company.FeeType = request.FeeType;
        company.FeeValue = (decimal)request.FeeValue;
        company.IsEnabled = request.IsEnabled;
        company.State = request.State;
        company.City = request.City;
        company.Neighborhood = request.Neighborhood;
        company.Street = request.Street;
        company.Number = request.Number;
        company.Latitude = request.Latitude;
        company.Longitude = request.Longitude;
        company.Slogan = request.Slogan;
        company.Description = request.Description;
        await _companyRepository.UpdateAsync(company);
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="company">Empresa atualizada.</param>
    /// <param name="user">Usuário autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Company company, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("Usuário não autenticado.", "Atualizar", "Empresa");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = "Atualização de Empresa",
            EntityId = company.Id,
            Details = $"Empresa {company.Name} atualizada com e-mail {company.Email} no estado {company.State}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}
