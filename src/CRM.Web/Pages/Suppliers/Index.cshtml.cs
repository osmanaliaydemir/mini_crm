using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Application.ExportImport;
using CRM.Application.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Suppliers;

public class IndexModel : PageModel
{
    private readonly ISupplierService _supplierService;
    private readonly IExportService _exportService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ISupplierService supplierService, IExportService exportService, ILogger<IndexModel> logger)
    {
        _supplierService = supplierService;
        _exportService = exportService;
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

    public async Task<IActionResult> OnGetExportAsync(string format = "excel", CancellationToken cancellationToken = default)
    {
        try
        {
            var suppliers = await _supplierService.GetAllAsync(cancellationToken);
            var suppliersList = suppliers.Cast<object>().ToList();

            byte[] fileBytes;
            string contentType;
            string fileName;

            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                fileBytes = await _exportService.ExportSuppliersToCsvAsync(suppliersList, cancellationToken);
                contentType = "text/csv";
                fileName = $"Tedarikciler_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            }
            else
            {
                fileBytes = await _exportService.ExportSuppliersToExcelAsync(suppliersList, cancellationToken);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"Tedarikciler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting suppliers");
            return RedirectToPage("./Index", new { error = "Export işlemi başarısız oldu." });
        }
    }
}

