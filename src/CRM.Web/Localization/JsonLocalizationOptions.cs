namespace CRM.Web.Localization;

public sealed class JsonLocalizationOptions
{
    public string ResourcesPath { get; set; } = "Resources";
    public bool FallBackToParentCultures { get; set; } = true;
}

