using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Auth;

/// <summary>
/// Caso de uso para redefinição de senha de usuários.
/// </summary>
public class ResetPasswordUseCase : BaseUseCase, IResetPasswordUseCase
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
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public ResetPasswordUseCase(
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IMessageService messageService,
        IConfiguration configuration,
        ILogger<ResetPasswordUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
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
    public async Task<string> RequestResetAsync(ResetPasswordRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            // Remover todas as linhas que usam _validator ou _confirmValidator

            // Busca e validação da empresa
            var company = await GetAndValidateCompanyAsync(request.Email);

            // Geração e armazenamento do token
            var resetToken = await GenerateAndStoreResetTokenAsync(request.Email);

            // Envio da mensagem
            await SendResetMessageAsync(company, resetToken);

            return resetToken;
        }, "Solicitação de Reset de Senha");
    }

    /// <summary>
    /// Confirma a redefinição de senha com um token e atualiza a senha do usuário.
    /// </summary>
    /// <param name="request">E-mail, token e nova senha.</param>
    public async Task ConfirmResetAsync(ResetPasswordConfirmRequest request)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos dados de entrada
            // Remover todas as linhas que usam _validator ou _confirmValidator

            // Busca e validação da empresa
            var company = await GetAndValidateCompanyAsync(request.Email);

            // Validação do token
            await ValidateResetTokenAsync(request.Email, request.ResetToken);

            // Atualização da senha
            await UpdatePasswordAsync(company, request.NewPassword);

            // Registro de auditoria
            await CreateAuditLogAsync(company);

            // Remoção do token usado
            await RemoveUsedTokenAsync(request.Email, request.ResetToken);
        }, "Confirmação de Reset de Senha");
    }

    /// <summary>
    /// Busca e valida a empresa.
    /// </summary>
    /// <param name="email">E-mail da empresa.</param>
    /// <returns>Empresa encontrada.</returns>
    private async Task<Domain.Entities.Company> GetAndValidateCompanyAsync(string email)
    {
        var company = await _companyRepository.GetByEmailAsync(email);
        EnsureEntityExists(company, "Empresa", email);
        return company!; // Garantido que não é null após EnsureEntityExists
    }

    /// <summary>
    /// Gera e armazena o token de reset.
    /// </summary>
    /// <param name="email">E-mail da empresa.</param>
    /// <returns>Token gerado.</returns>
    private async Task<string> GenerateAndStoreResetTokenAsync(string email)
    {
        var resetToken = GenerateResetToken();
        var tokenEntity = new PasswordResetToken
        {
            Email = email,
            Token = resetToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };
        await _passwordResetTokenRepository.AddAsync(tokenEntity);
        return resetToken;
    }

    /// <summary>
    /// Envia a mensagem de reset.
    /// </summary>
    /// <param name="company">Empresa.</param>
    /// <param name="resetToken">Token de reset.</param>
    private async Task SendResetMessageAsync(Domain.Entities.Company company, string resetToken)
    {
        var message = $"Seu token de redefinição de senha é: {resetToken}";
        try
        {
            if (!string.IsNullOrEmpty(company.PhoneNumber))
                await _messageService.SendWhatsAppAsync(company.PhoneNumber, message);
            else
                await _messageService.SendEmailAsync(company.Email, "Redefinição de Senha", message);
        }
        catch (HttpRequestException ex)
        {
            await _messageService.SendEmailAsync(company.Email, "Redefinição de Senha", $"{message}\nWhatsApp falhou: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida o token de reset.
    /// </summary>
    /// <param name="email">E-mail da empresa.</param>
    /// <param name="resetToken">Token a ser validado.</param>
    private async Task ValidateResetTokenAsync(string email, string resetToken)
    {
        var tokenEntity = await _passwordResetTokenRepository.GetByEmailAndTokenAsync(email, resetToken);
        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            throw new BusinessRuleException("Token inválido ou expirado.", "TOKEN_VALIDATION");
    }

    /// <summary>
    /// Atualiza a senha da empresa.
    /// </summary>
    /// <param name="company">Empresa.</param>
    /// <param name="newPassword">Nova senha.</param>
    private async Task UpdatePasswordAsync(Domain.Entities.Company company, string newPassword)
    {
        company.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _companyRepository.UpdateAsync(company);
    }

    /// <summary>
    /// Cria o log de auditoria.
    /// </summary>
    /// <param name="company">Empresa.</param>
    private async Task CreateAuditLogAsync(Domain.Entities.Company company)
    {
        if (company.Role == Role.Tenant)
        {
            await _auditLogRepository.AddAsync(new AuditLog
            {
                UserId = "System",
                Action = "Redefinição de Senha",
                EntityId = company.Id,
                Details = $"Senha da empresa {company.Name} redefinida.",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Remove o token usado.
    /// </summary>
    /// <param name="email">E-mail da empresa.</param>
    /// <param name="resetToken">Token usado.</param>
    private async Task RemoveUsedTokenAsync(string email, string resetToken)
    {
        var tokenEntity = await _passwordResetTokenRepository.GetByEmailAndTokenAsync(email, resetToken);
        if (tokenEntity != null)
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