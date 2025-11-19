using CRM.Application.Dashboard;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDashboardService _dashboardService;
    private readonly UserManager<ApplicationUser> _userManager;

    public const string SupplierUnspecifiedToken = "UNSPECIFIED";

    public DashboardData DashboardData { get; private set; } = null!;
    public Dictionary<Guid, string> UserNames { get; private set; } = new();

    // Backward compatibility properties
    public DashboardSummary Summary => DashboardData.Summary;
    public IReadOnlyList<StatusSummary> ShipmentStatusSummaries => DashboardData.ShipmentStatusSummaries;
    public IReadOnlyList<TimeSeriesPoint> ShipmentMonthlyTrend => DashboardData.ShipmentMonthlyTrend;
    public IReadOnlyList<CategoryPoint> SupplierCountryBreakdown => DashboardData.SupplierCountryBreakdown;
    public IReadOnlyList<TimeSeriesPoint> WarehouseVolumeTrend => DashboardData.WarehouseVolumeTrend;
    public IReadOnlyList<CashFlowPoint> CashFlowTrend => DashboardData.CashFlowTrend;
    public IReadOnlyList<TimeSeriesPoint> CustomerInteractionTrend => DashboardData.CustomerInteractionTrend;
    public Dictionary<string, IReadOnlyList<ActivityEvent>> ActivityFeed => DashboardData.ActivityFeed;

    public IndexModel(ILogger<IndexModel> logger, IDashboardService dashboardService, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _dashboardService = dashboardService;
        _userManager = userManager;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            DashboardData = await _dashboardService.GetDashboardDataAsync(cancellationToken);

            // Load user names for tasks
            var userIds = DashboardData.TodayTasks
                .Where(t => t.AssignedToUserId.HasValue)
                .Select(t => t.AssignedToUserId!.Value)
                .Distinct()
                .ToList();

            foreach (var userId in userIds)
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null)
                {
                    UserNames[userId] = user.UserName ?? user.Email ?? string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            // Hata durumunda boş değerlerle devam et
            DashboardData = new DashboardData();
        }
    }
}
