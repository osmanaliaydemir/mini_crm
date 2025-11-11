using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace CRM.Web.Localization;

internal sealed class JsonStringLocalizer : IStringLocalizer
{
    private readonly string _resourceName;
    private readonly JsonLocalizationOptions _options;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<JsonStringLocalizer> _logger;
    private readonly CultureInfo? _culture;

    private readonly ConcurrentDictionary<string, Lazy<IDictionary<string, string>>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public JsonStringLocalizer(
        string resourceName,
        JsonLocalizationOptions options,
        IFileProvider fileProvider,
        ILogger<JsonStringLocalizer> logger,
        CultureInfo? culture = null)
    {
        _resourceName = resourceName;
        _options = options;
        _fileProvider = fileProvider;
        _logger = logger;
        _culture = culture;
    }

    public LocalizedString this[string name]
        => GetString(name);

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var localized = GetString(name);
            var value = localized.ResourceNotFound
                ? localized.Value
                : string.Format(localized.Value, arguments);
            return new LocalizedString(name, value, localized.ResourceNotFound, localized.SearchedLocation);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = _culture ?? CultureInfo.CurrentUICulture;
        var searchedCultureNames = BuildCandidateCultures(culture, includeParentCultures).Distinct();

        foreach (var cultureName in searchedCultureNames)
        {
            var dictionary = LoadCultureDictionary(cultureName);
            foreach (var pair in dictionary)
            {
                yield return new LocalizedString(pair.Key, pair.Value, false);
            }
        }
    }

    public IStringLocalizer WithCulture(CultureInfo culture)
        => new JsonStringLocalizer(_resourceName, _options, _fileProvider, _logger, culture);

    private LocalizedString GetString(string name)
    {
        var culture = _culture ?? CultureInfo.CurrentUICulture;
        foreach (var cultureName in BuildCandidateCultures(culture, _options.FallBackToParentCultures))
        {
            var dictionary = LoadCultureDictionary(cultureName);
            if (dictionary.TryGetValue(name, out var value))
            {
                return new LocalizedString(name, value, false);
            }
        }

        _logger.LogDebug("Missing localization for key '{Key}' in resource '{Resource}' and culture '{Culture}'.", name, _resourceName, culture);
        return new LocalizedString(name, name, true);
    }

    private IDictionary<string, string> LoadCultureDictionary(string cultureName)
    {
        return _cache.GetOrAdd(cultureName, c => new Lazy<IDictionary<string, string>>(() =>
        {
            var searchPaths = BuildSearchPaths(cultureName);
            foreach (var path in searchPaths)
            {
                var file = _fileProvider.GetFileInfo(path);
                if (file.Exists)
                {
                    try
                    {
                        using var stream = file.CreateReadStream();
                        using var document = JsonDocument.Parse(stream);
                        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        foreach (var element in document.RootElement.EnumerateObject())
                        {
                            if (element.Value.ValueKind == JsonValueKind.String)
                            {
                                result[element.Name] = element.Value.GetString() ?? string.Empty;
                            }
                        }

                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse localization file {Path}.", path);
                        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                }
            }

            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        })).Value;
    }

    private IEnumerable<string> BuildSearchPaths(string cultureName)
    {
        var basePath = _options.ResourcesPath?.TrimEnd('/', '\\') ?? "Resources";
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            candidates.Add($"{basePath}/{_resourceName}.{cultureName}.json");

            if (cultureName.Contains('-'))
            {
                var neuter = cultureName.Split('-')[0];
                candidates.Add($"{basePath}/{_resourceName}.{neuter}.json");
            }
        }

        candidates.Add($"{basePath}/{_resourceName}.json");
        return candidates;
    }

    private IEnumerable<string> BuildCandidateCultures(CultureInfo culture, bool includeParents)
    {
        yield return culture.Name;

        if (includeParents && culture.Name.Contains('-'))
        {
            yield return culture.Parent.Name;
        }

        yield return string.Empty;
    }
}

