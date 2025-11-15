namespace CRM.Application.Common;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string htmlMessage, CancellationToken cancellationToken = default);
}

