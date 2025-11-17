using System.Text.Encodings.Web;
using System.Text.Json;
using CRM.Application.Customers;
using CRM.Application.ExportImport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly ICustomerService _customerService;
    private readonly IExportService _exportService;
    private readonly ILogger<IndexModel> _logger;

    public const string UnspecifiedSegmentLabel = "__UNSPECIFIED__";

    public IndexModel(ICustomerService customerService, IExportService exportService, ILogger<IndexModel> logger)
    {
        _customerService = customerService;
        _exportService = exportService;
        _logger = logger;
    }

    public string? Search { get; set; }
    public IReadOnlyList<CustomerListItemDto> Customers { get; private set; } = Array.Empty<CustomerListItemDto>();
    public int TotalCustomers { get; private set; }
    public int DistinctSegmentCount { get; private set; }
    public int NewCustomersCount { get; private set; }
    public int RecentInteractionsCount { get; private set; }
    public string TopSegmentName { get; private set; } = UnspecifiedSegmentLabel;
    public int TopSegmentCustomerCount { get; private set; }
    public IReadOnlyList<CustomerSegmentStat> CustomerSegmentStats { get; private set; } = Array.Empty<CustomerSegmentStat>();
    public IReadOnlyList<TopCustomerStat> TopCustomerStats { get; private set; } = Array.Empty<TopCustomerStat>();
    public string MonthlyInteractionLabelsJson { get; private set; } = "[]";
    public string MonthlyInteractionDataJson { get; private set; } = "[]";

    public async Task OnGetAsync(string? search, CancellationToken cancellationToken)
    {
        try
        {
            Search = search;

            var dashboardData = await _customerService.GetDashboardDataAsync(search, cancellationToken);

            Customers = dashboardData.Customers;
            TotalCustomers = dashboardData.TotalCustomers;
            NewCustomersCount = dashboardData.NewCustomersCount;
            RecentInteractionsCount = dashboardData.RecentInteractionsCount;
            DistinctSegmentCount = dashboardData.DistinctSegmentCount;
            TopSegmentName = dashboardData.TopSegmentName;
            TopSegmentCustomerCount = dashboardData.TopSegmentCustomerCount;
            CustomerSegmentStats = dashboardData.CustomerSegmentStats;
            TopCustomerStats = dashboardData.TopCustomerStats;

            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            MonthlyInteractionLabelsJson = JsonSerializer.Serialize(dashboardData.MonthlyInteractionLabels, jsonOptions);
            MonthlyInteractionDataJson = JsonSerializer.Serialize(dashboardData.MonthlyInteractionData, jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customer dashboard data");
            // Hata durumunda boş değerlerle devam et
            Customers = Array.Empty<CustomerListItemDto>();
        }
    }

    public async Task<IActionResult> OnGetExportAsync(string format = "excel", string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var customers = await _customerService.GetAllAsync(search, cancellationToken);
            var customersList = customers.Cast<object>().ToList();

            byte[] fileBytes;
            string contentType;
            string fileName;

            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                fileBytes = await _exportService.ExportCustomersToCsvAsync(customersList, cancellationToken);
                contentType = "text/csv";
                fileName = $"Musteriler_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            }
            else
            {
                fileBytes = await _exportService.ExportCustomersToExcelAsync(customersList, cancellationToken);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"Musteriler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting customers");
            return RedirectToPage("./Index", new { error = "Export işlemi başarısız oldu." });
        }
    }

    public static string NormalizeSegment(string? segment) =>
        string.IsNullOrWhiteSpace(segment) ? UnspecifiedSegmentLabel : segment.Trim();
}

