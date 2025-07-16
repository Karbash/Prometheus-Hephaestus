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
/// Caso de uso para atualiza��o de empresas.
/// </summary>
public class UpdateCompanyUseCase : BaseUseCase, IUpdateCompanyUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="UpdateCompanyUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    /// <param name="auditLogRepository">Reposit�rio de logs de auditoria.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
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
    /// Executa a atualiza��o de uma empresa.
    /// </summary>
    /// <param name="id">ID da empresa.</param>
    /// <param name="request">Dados atualizados da empresa.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    public async Task ExecuteAsync(string id, UpdateCompanyRequest request, ClaimsPrincipal user)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos dados de entrada
            // Remover todas as linhas que usam _validator

            // Valida��o de autoriza��o
            ValidateAuthorization(user);

            // Busca e valida��o da empresa
            var company = await GetAndValidateCompanyAsync(id);

            // Valida��o das regras de neg�cio
            await ValidateBusinessRulesAsync(request, id);

            // Atualiza��o da empresa
            await UpdateCompanyAsync(company, request);

            // Registro de auditoria
            await CreateAuditLogAsync(company, user);
        }, "Atualiza��o de Empresa");
    }

    /// <summary>
    /// Valida a autoriza��o do usu�rio.
    /// </summary>
    /// <param name="user">Usu�rio autenticado.</param>
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
        return company!; // Garantido que n�o � null ap�s EnsureEntityExists
    }

    /// <summary>
    /// Valida as regras de neg�cio.
    /// </summary>
    /// <param name="request">Requisi��o com os dados.</param>
    /// <param name="id">ID da empresa.</param>
    private async Task ValidateBusinessRulesAsync(UpdateCompanyRequest request, string id)
    {
        var existingByEmail = await _companyRepository.GetByEmailAsync(request.Email);
        if (existingByEmail != null && existingByEmail.Id != id)
            throw new ConflictException("E-mail j� registrado.", "Empresa", "Email", request.Email);

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            var existingByPhone = await _companyRepository.GetByPhoneNumberAsync(request.PhoneNumber);
            if (existingByPhone != null && existingByPhone.Id != id)
                throw new ConflictException("Telefone j� registrado.", "Empresa", "PhoneNumber", request.PhoneNumber);
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
        company.Slogan = request.Slogan;
        company.Description = request.Description;
        await _companyRepository.UpdateAsync(company);
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="company">Empresa atualizada.</param>
    /// <param name="user">Usu�rio autenticado.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Company company, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("Usu�rio n�o autenticado.", "Atualizar", "Empresa");

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = "Atualiza��o de Empresa",
            EntityId = company.Id,
            Details = $"Empresa {company.Name} atualizada com e-mail {company.Email}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}
