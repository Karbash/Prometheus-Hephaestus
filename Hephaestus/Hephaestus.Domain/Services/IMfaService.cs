namespace Hephaestus.Domain.Services;

public interface IMfaService
{
    Task<bool> ValidateMfaCodeAsync(string email, string mfaCode);
    Task<string> GenerateMfaSecretAsync(string email);
}