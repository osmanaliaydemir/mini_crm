using CRM.Application.Common;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Email;

public sealed class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        // TODO: Production'da gerçek SMTP servisi entegre edilmeli (SendGrid, SMTP, vb.)
        // Şimdilik log'a yazıyoruz. Production'da bu kısım değiştirilmeli.
        
        _logger.LogInformation(
            "E-posta gönderiliyor - Alıcı: {To}, Konu: {Subject}",
            to, subject);

        // Production implementasyonu için örnek:
        // - SendGrid, MailKit, SMTP client kullanılabilir
        // - appsettings.json'dan SMTP ayarları okunabilir
        // - Async olarak e-posta kuyruğuna eklenebilir (RabbitMQ, Azure Service Bus, vb.)

        return Task.CompletedTask;
    }
}

