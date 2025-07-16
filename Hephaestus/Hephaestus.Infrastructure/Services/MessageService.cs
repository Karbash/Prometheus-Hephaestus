using Hephaestus.Domain.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Hephaestus.Infrastructure.Services;

/// <summary>
/// Serviço para envio de mensagens por e-mail e WhatsApp.
/// </summary>
public class MessageService : IMessageService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="MessageService"/>.
    /// </summary>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="httpClient">Cliente HTTP para chamadas à API do WhatsApp.</param>
    public MessageService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Envia uma mensagem por e-mail usando configurações SMTP.
    /// </summary>
    /// <param name="email">Endereço de e-mail do destinatário.</param>
    /// <param name="subject">Assunto do e-mail.</param>
    /// <param name="message">Corpo da mensagem.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    /// <exception cref="SmtpException">Falha ao enviar o e-mail.</exception>
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var smtpClient = new SmtpClient(smtpSettings["Host"])
        {
            Port = int.Parse(smtpSettings["Port"]!),
            EnableSsl = bool.Parse(smtpSettings["EnableSsl"]!),
            UseDefaultCredentials = false,
            Credentials = new System.Net.NetworkCredential(
                smtpSettings["Username"],
                smtpSettings["Password"]
            )
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(
                smtpSettings["FromEmail"]!,
                smtpSettings["FromName"]
            ),
            Subject = subject,
            Body = message,
            IsBodyHtml = false
        };

        mailMessage.To.Add(email);

        await smtpClient.SendMailAsync(mailMessage);
    }

    /// <summary>
    /// Envia uma mensagem via WhatsApp usando a API do WhatsApp.
    /// </summary>
    /// <param name="phoneNumber">Número de telefone do destinatário (formato internacional).</param>
    /// <param name="message">Mensagem a ser enviada.</param>
    /// <returns>Task representando a operação assíncrona.</returns>
    /// <exception cref="HttpRequestException">Falha na chamada à API do WhatsApp.</exception>
    public async Task SendWhatsAppAsync(string phoneNumber, string message)
    {
        var whatsappSettings = _configuration.GetSection("WhatsAppSettings");
        var accessToken = whatsappSettings["AccessToken"];
        var phoneNumberId = whatsappSettings["PhoneNumberId"];
        var apiUrl = $"https://graph.facebook.com/v20.0/{phoneNumberId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = phoneNumber,
            type = "text",
            text = new { body = message }
        };

        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.PostAsync(apiUrl, content);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"WhatsApp API request failed: {response.StatusCode}. Details: {errorContent}");
        }
    }
}
