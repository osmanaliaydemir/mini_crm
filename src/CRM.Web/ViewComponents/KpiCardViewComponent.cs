using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.ViewComponents;

public class KpiCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string label,
        object value,
        string? meta = null,
        string? trend = null,
        string? trendType = null,
        string? dataKpi = null)
    {
        var model = new KpiCardViewModel
        {
            Label = label,
            Value = value?.ToString() ?? string.Empty,
            Meta = meta,
            Trend = trend,
            TrendType = trendType ?? "neutral", // positive, negative, warning, neutral
            DataKpi = dataKpi
        };

        return View(model);
    }

    public class KpiCardViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Meta { get; set; }
        public string? Trend { get; set; }
        public string TrendType { get; set; } = "neutral";
        public string? DataKpi { get; set; }
    }
}

