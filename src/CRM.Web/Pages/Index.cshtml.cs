using CRM.Application.Dashboard;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDashboardService _dashboardService;

    public const string SupplierUnspecifiedToken = "UNSPECIFIED";

    public DashboardData DashboardData { get; private set; } = null!;

    // Backward compatibility properties
    public DashboardSummary Summary => DashboardData.Summary;
    public IReadOnlyList<StatusSummary> ShipmentStatusSummaries => DashboardData.ShipmentStatusSummaries;
    public IReadOnlyList<TimeSeriesPoint> ShipmentMonthlyTrend => DashboardData.ShipmentMonthlyTrend;
    public IReadOnlyList<CategoryPoint> SupplierCountryBreakdown => DashboardData.SupplierCountryBreakdown;
    public IReadOnlyList<TimeSeriesPoint> WarehouseVolumeTrend => DashboardData.WarehouseVolumeTrend;
    public IReadOnlyList<CashFlowPoint> CashFlowTrend => DashboardData.CashFlowTrend;
    public IReadOnlyList<TimeSeriesPoint> CustomerInteractionTrend => DashboardData.CustomerInteractionTrend;
    public Dictionary<string, IReadOnlyList<ActivityEvent>> ActivityFeed => DashboardData.ActivityFeed;

    public IndexModel(ILogger<IndexModel> logger, IDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            DashboardData = await _dashboardService.GetDashboardDataAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            // Hata durumunda boş değerlerle devam et
            DashboardData = new DashboardData();
        }
    }
}
