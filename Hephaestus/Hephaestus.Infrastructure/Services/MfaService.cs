using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using OtpNet;
using System.Security.Cryptography;

namespace Hephaestus.Infrastructure.Services;

/// <summary>
/// Serviço para gerenciamento de autenticação multifator (MFA) usando TOTP.
/// </summary>
public class MfaService : IMfaService
{
    private readonly ICompanyRepository _companyRepository;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="MfaService"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    public MfaService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Valida um código MFA para um usuário.
    /// </summary>
    /// <param name="email">E-mail do usuário.</param>
    /// <param name="mfaCode">Código MFA fornecido.</param>
    /// <returns>True se o código for válido, False caso contrário.</returns>
    public async Task<bool> ValidateMfaCodeAsync(string email, string mfaCode)
    {
        var company = await _companyRepository.GetByEmailAsync(email);
        if (company == null || string.IsNullOrEmpty(company.MfaSecret))
            return false;

        var totp = new Totp(Base32Encoding.ToBytes(company.MfaSecret));
        return totp.VerifyTotp(mfaCode, out _);
    }

    /// <summary>
    /// Gera um segredo TOTP para configuração de MFA e atualiza o usuário.
    /// </summary>
    /// <param name="email">E-mail do usuário.</param>
    /// <returns>Segredo TOTP gerado.</returns>
    /// <exception cref="InvalidOperationException">E-mail não encontrado.</exception>
    public async Task<string> GenerateMfaSecretAsync(string email)
    {
        var company = await _companyRepository.GetByEmailAsync(email);
        if (company == null)
            throw new InvalidOperationException("E-mail não encontrado.");

        var randomBytes = RandomNumberGenerator.GetBytes(20);
        var secret = Base32Encoding.ToString(randomBytes);
        company.MfaSecret = secret;
        await _companyRepository.UpdateAsync(company);

        return secret;
    }
}
