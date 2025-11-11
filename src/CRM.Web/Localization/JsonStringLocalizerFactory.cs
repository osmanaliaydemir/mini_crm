using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Web.Localization;

internal sealed class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly JsonLocalizationOptions _options;
    private readonly IFileProvider _fileProvider;
    private readonly ILoggerFactory _loggerFactory;

    public JsonStringLocalizerFactory(
        IOptions<JsonLocalizationOptions> options,
        IFileProvider fileProvider,
        ILoggerFactory loggerFactory)
    {
        _options = options.Value;
        _fileProvider = fileProvider;
        _loggerFactory = loggerFactory;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var baseName = resourceSource.Name;
        return CreateInternal(baseName);
    }

    public IStringLocalizer Create(string baseName, string? location)
    {
        var resourceName = baseName.Split('.').LastOrDefault() ?? baseName;
        return CreateInternal(resourceName);
    }

    private IStringLocalizer CreateInternal(string resourceName)
    {
        var logger = _loggerFactory.CreateLogger<JsonStringLocalizer>();
        return new JsonStringLocalizer(resourceName, _options, _fileProvider, logger);
    }
}

