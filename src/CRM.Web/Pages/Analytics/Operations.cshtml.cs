using CRM.Application.Analytics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace CRM.Web.Pages.Analytics;

public class OperationsModel : PageModel
{
    private readonly IAnalyticsService _analyticsService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public OperationsModel(IAnalyticsService analyticsService, IStringLocalizer<SharedResource> localizer)
    {
        _analyticsService = analyticsService;
        _localizer = localizer;
    }

    public AnalyticsData AnalyticsData { get; private set; } = null!;

    // Backward compatibility properties
    public OperationsSummary Summary => AnalyticsData.Summary;
    public IReadOnlyList<StatusBreakdownItem> StatusBreakdown => AnalyticsData.StatusBreakdown;
    public IReadOnlyList<CompletionTrendPoint> CompletionTrend => AnalyticsData.CompletionTrend;
    public IReadOnlyList<WarehouseThroughputItem> WarehouseThroughput => AnalyticsData.WarehouseThroughput;
    public IReadOnlyList<DelayShipmentItem> DelayShipments => AnalyticsData.DelayShipments;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        AnalyticsData = await _analyticsService.GetOperationsAnalyticsAsync(cancellationToken);
    }
}
