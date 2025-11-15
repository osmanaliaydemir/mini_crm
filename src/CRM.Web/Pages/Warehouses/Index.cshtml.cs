using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Application.Warehouses;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Warehouses;

public class IndexModel : PageModel
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<IndexModel> _logger;

    public const string UnspecifiedLocationLabel = "__UNSPECIFIED__";

    public IndexModel(IWarehouseService warehouseService, ILogger<IndexModel> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    public IReadOnlyList<WarehouseDto> Warehouses { get; private set; } = Array.Empty<WarehouseDto>();
    public int TotalWarehouses { get; private set; }
    public int DistinctLocationCount { get; private set; }
    public int ActiveWarehousesCount { get; private set; }
    public int RecentUnloadingCount { get; private set; }
    public string TopLocationName { get; private set; } = UnspecifiedLocationLabel;
    public int TopLocationWarehouseCount { get; private set; }
    public IReadOnlyList<WarehouseLocationStat> WarehouseLocationStats { get; private set; } = Array.Empty<WarehouseLocationStat>();
    public IReadOnlyList<TopWarehouseStat> TopWarehouseStats { get; private set; } = Array.Empty<TopWarehouseStat>();
    public string MonthlyVolumeLabelsJson { get; private set; } = "[]";
    public string MonthlyVolumeDataJson { get; private set; } = "[]";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var dashboardData = await _warehouseService.GetDashboardDataAsync(cancellationToken);

            Warehouses = dashboardData.Warehouses;
            TotalWarehouses = dashboardData.TotalWarehouses;
            DistinctLocationCount = dashboardData.DistinctLocationCount;
            ActiveWarehousesCount = dashboardData.ActiveWarehousesCount;
            RecentUnloadingCount = dashboardData.RecentUnloadingCount;
            TopLocationName = dashboardData.TopLocationName;
            TopLocationWarehouseCount = dashboardData.TopLocationWarehouseCount;
            WarehouseLocationStats = dashboardData.WarehouseLocationStats;
            TopWarehouseStats = dashboardData.TopWarehouseStats;

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            MonthlyVolumeLabelsJson = JsonSerializer.Serialize(dashboardData.MonthlyVolumeLabels, jsonOptions);
            MonthlyVolumeDataJson = JsonSerializer.Serialize(dashboardData.MonthlyVolumeData, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading warehouse dashboard data");
            Warehouses = Array.Empty<WarehouseDto>();
        }
    }
}

