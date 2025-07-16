using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using OtpNet;
using System.Security.Cryptography;

namespace Hephaestus.Infrastructure.Services;

/// <summary>
/// Servi�o para gerenciamento de autentica��o multifator (MFA) usando TOTP.
/// </summary>
public class MfaService : IMfaService
{
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="MfaService"/>.
    /// </summary>
    /// <param name="companyRepository">Reposit�rio de empresas.</param>
    public MfaService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Valida um c�digo MFA para um usu�rio.
    /// </summary>
    /// <param name="email">E-mail do usu�rio.</param>
    /// <param name="mfaCode">C�digo MFA fornecido.</param>
    /// <returns>True se o c�digo for v�lido, False caso contr�rio.</returns>
    public async Task<bool> ValidateMfaCodeAsync(string email, string mfaCode)
    {
        var company = await _companyRepository.GetByEmailAsync(email);
        if (company == null || string.IsNullOrEmpty(company.MfaSecret))
            return false;

        var totp = new Totp(Base32Encoding.ToBytes(company.MfaSecret));
        return totp.VerifyTotp(mfaCode, out _);
    }

    /// <summary>
    /// Gera um segredo TOTP para configura��o de MFA e atualiza o usu�rio.
    /// </summary>
    /// <param name="email">E-mail do usu�rio.</param>
    /// <returns>Segredo TOTP gerado.</returns>
    /// <exception cref="InvalidOperationException">E-mail n�o encontrado.</exception>
    public async Task<string> GenerateMfaSecretAsync(string email)
    {
        var company = await _companyRepository.GetByEmailAsync(email);
        if (company == null)
            throw new InvalidOperationException("E-mail n�o encontrado.");

        var randomBytes = RandomNumberGenerator.GetBytes(20);
        var secret = Base32Encoding.ToString(randomBytes);
        company.MfaSecret = secret;
        await _companyRepository.UpdateAsync(company);

        return secret;
    }
}
