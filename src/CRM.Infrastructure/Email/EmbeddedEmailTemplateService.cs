using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using CRM.Application.Common;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Email;

public sealed class EmbeddedEmailTemplateService : IEmailTemplateService
{
    private static readonly Assembly ResourceAssembly = typeof(EmbeddedEmailTemplateService).Assembly;
    private readonly ILogger<EmbeddedEmailTemplateService> _logger;
    private readonly ConcurrentDictionary<string, string> _templateCache = new();

    public EmbeddedEmailTemplateService(ILogger<EmbeddedEmailTemplateService> logger)
    {
        _logger = logger;
    }

    public Task<string> RenderTemplateAsync(string templateName, IDictionary<string, string> placeholders,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateName))
        {
            throw new ArgumentException("Template name cannot be empty.", nameof(templateName));
        }

        var template = _templateCache.GetOrAdd(templateName, LoadTemplate);

        var content = new StringBuilder(template);
        if (placeholders != null)
        {
            foreach (var (key, value) in placeholders)
            {
                content.Replace($"{{{{{key}}}}}", value ?? string.Empty);
            }
        }

        return Task.FromResult(content.ToString());
    }

    private string LoadTemplate(string templateName)
    {
        var resourceName = $"CRM.Infrastructure.Email.Templates.{templateName}.html";
        using var stream = ResourceAssembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            _logger.LogError("E-posta şablonu bulunamadı: {Template}", resourceName);
            throw new InvalidOperationException($"Email template '{templateName}' could not be found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}


