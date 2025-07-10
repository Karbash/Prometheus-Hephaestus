namespace Hephaestus.Domain.Services;

public interface IMessageService
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendWhatsAppAsync(string phoneNumber, string message);
}