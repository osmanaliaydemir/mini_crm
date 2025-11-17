namespace CRM.Application.Common;

public interface IEmailTemplateService
{
    Task<string> RenderTemplateAsync(string templateName, IDictionary<string, string> placeholders,
        CancellationToken cancellationToken = default);
}


