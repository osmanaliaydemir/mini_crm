using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Application.Suppliers;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Suppliers;

public class IndexModel : PageModel
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ISupplierService supplierService, ILogger<IndexModel> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    public IReadOnlyList<SupplierDto> Suppliers { get; private set; } = Array.Empty<SupplierDto>();
    public int TotalSuppliers { get; private set; }
    public int DistinctCountryCount { get; private set; }
    public int RecentSuppliersCount { get; private set; }
    public string TopCountryName { get; private set; } = "-";
    public int TopCountrySupplierCount { get; private set; }
    public IReadOnlyList<SupplierCountryStat> SupplierCountryStats { get; private set; } = Array.Empty<SupplierCountryStat>();
    public string SupplierCountryChartLabelsJson { get; private set; } = "[]";
    public string SupplierCountryChartDataJson { get; private set; } = "[]";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var dashboardData = await _supplierService.GetDashboardDataAsync(cancellationToken);

            Suppliers = dashboardData.Suppliers;
            TotalSuppliers = dashboardData.TotalSuppliers;
            DistinctCountryCount = dashboardData.DistinctCountryCount;
            RecentSuppliersCount = dashboardData.RecentSuppliersCount;
            TopCountryName = dashboardData.TopCountryName;
            TopCountrySupplierCount = dashboardData.TopCountrySupplierCount;
            SupplierCountryStats = dashboardData.SupplierCountryStats;

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            SupplierCountryChartLabelsJson = JsonSerializer.Serialize(dashboardData.CountryChartLabels, jsonOptions);
            SupplierCountryChartDataJson = JsonSerializer.Serialize(dashboardData.CountryChartData, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading supplier dashboard data");
            Suppliers = Array.Empty<SupplierDto>();
        }
    }
}

