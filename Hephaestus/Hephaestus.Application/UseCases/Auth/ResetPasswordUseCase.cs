using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para redefinição de senha de usuários.
/// </summary>
public class ResetPasswordUseCase : IResetPasswordUseCase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IMessageService _messageService;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="ResetPasswordUseCase"/>.
    /// </summary>
    /// <param name="companyRepository">Repositório de empresas.</param>
    /// <param name="auditLogRepository">Repositório de logs de auditoria.</param>
    /// <param name="passwordResetTokenRepository">Repositório de tokens de redefinição de senha.</param>
    /// <param name="messageService">Serviço de envio de mensagens.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    public ResetPasswordUseCase(
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IMessageService messageService,
        IConfiguration configuration)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _messageService = messageService;
        _configuration = configuration;
    }

    /// <summary>
    /// Solicita um token para redefinição de senha, enviando-o por WhatsApp ou e-mail.
    /// </summary>
    /// <param name="request">E-mail do usuário.</param>
    /// <returns>Token de redefinição de senha.</returns>
    /// <exception cref="InvalidOperationException">E-mail não encontrado.</exception>
    public async Task<string> RequestResetAsync(ResetPasswordRequest request)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        if (company == null)
            throw new InvalidOperationException("E-mail não encontrado.");

        var resetToken = GenerateResetToken();
        var tokenEntity = new PasswordResetToken
        {
            Email = request.Email,
            Token = resetToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
        await _passwordResetTokenRepository.AddAsync(tokenEntity);

        var message = $"Seu token de redefinição de senha é: {resetToken}";
        try
        {
            if (!string.IsNullOrEmpty(company.PhoneNumber))
                await _messageService.SendWhatsAppAsync(company.PhoneNumber, message);
            else
                await _messageService.SendEmailAsync(request.Email, "Redefinição de Senha", message);
        }
        catch (HttpRequestException ex)
        {
            await _messageService.SendEmailAsync(request.Email, "Redefinição de Senha", $"{message}\nWhatsApp falhou: {ex.Message}");
        }

        return resetToken;
    }

    /// <summary>
    /// Confirma a redefinição de senha com um token e atualiza a senha do usuário.
    /// </summary>
    /// <param name="request">E-mail, token e nova senha.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    /// <exception cref="KeyNotFoundException">E-mail não encontrado.</exception>
    /// <exception cref="InvalidOperationException">Token inválido ou expirado.</exception>
    public async Task ConfirmResetAsync(ResetPasswordConfirmRequest request)
    {
        var company = await _companyRepository.GetByEmailAsync(request.Email);
        if (company == null)
            throw new KeyNotFoundException("E-mail não encontrado.");

        var tokenEntity = await _passwordResetTokenRepository.GetByEmailAndTokenAsync(request.Email, request.ResetToken);
        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Token inválido ou expirado.");

        company.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _companyRepository.UpdateAsync(company);

        if (company.Role == Role.Tenant)
        {
            var auditLog = new AuditLog
            {
                AdminId = "System",
                Action = "Redefinição de Senha",
                EntityId = company.Id,
                Details = $"Senha da empresa {company.Name} redefinida.",
                CreatedAt = DateTime.UtcNow
            };
            await _auditLogRepository.AddAsync(auditLog);
        }

        await _passwordResetTokenRepository.DeleteAsync(tokenEntity);
    }

    /// <summary>
    /// Gera um token de redefinição de senha seguro.
    /// </summary>
    /// <returns>Token de 6 caracteres.</returns>
    private string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Substring(0, 6);
    }
}