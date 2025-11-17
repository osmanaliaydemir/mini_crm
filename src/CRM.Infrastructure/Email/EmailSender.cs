using System.Net;
using System.Net.Mail;
using CRM.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Email;

public sealed class EmailSender : IEmailSender
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IApplicationDbContext dbContext, ILogger<EmailSender> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlMessage, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("E-posta alıcısı belirtilmelidir.", nameof(to));
        }

        var settings = await _dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            throw new InvalidOperationException("E-posta gönderimi için sistem ayarları bulunamadı.");
        }

        if (!settings.EnableEmailNotifications)
        {
            _logger.LogWarning("E-posta bildirimi devre dışı bırakıldığı için mesaj gönderilmedi. Alıcı: {Recipient}", to);
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.SmtpHost) ||
            string.IsNullOrWhiteSpace(settings.SmtpUsername) ||
            string.IsNullOrWhiteSpace(settings.SmtpPassword))
        {
            throw new InvalidOperationException("SMTP ayarları eksik. Lütfen Sistem Ayarları > E-posta sekmesini doldurun.");
        }

        var fromAddress = settings.SmtpFromEmail ?? settings.SmtpUsername;
        if (string.IsNullOrWhiteSpace(fromAddress))
        {
            throw new InvalidOperationException("Gönderen e-posta adresi belirlenemedi.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, settings.SmtpFromName ?? settings.CompanyName ?? "CRM"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(to));

        using var smtpClient = new SmtpClient(settings.SmtpHost, settings.SmtpPort ?? 587)
        {
            EnableSsl = settings.SmtpEnableSsl,
            Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
            Timeout = 15000
        };

        await smtpClient.SendMailAsync(message, cancellationToken);

        _logger.LogInformation("E-posta gönderildi. Alıcı: {Recipient}, Konu: {Subject}", to, subject);
    }
}

