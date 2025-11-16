using CRM.Application.ExportImport;
using CRM.Application.Shipments;
using CRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Shipments;

public class IndexModel : PageModel
{
    private readonly IShipmentService _shipmentService;
    private readonly IExportService _exportService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IShipmentService shipmentService,
        IExportService exportService,
        ILogger<IndexModel> logger)
    {
        _shipmentService = shipmentService;
        _exportService = exportService;
        _logger = logger;
    }

    public IReadOnlyList<ShipmentListItemDto> Shipments { get; private set; } = Array.Empty<ShipmentListItemDto>();
    public int TotalShipments { get; private set; }
    public int ActiveShipments { get; private set; }
    public int DeliveredShipments { get; private set; }
    public int CustomsShipments { get; private set; }
    public IReadOnlyList<StatusSummary> StatusSummaries { get; private set; } = Array.Empty<StatusSummary>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var dashboardData = await _shipmentService.GetDashboardDataAsync(cancellationToken);

            Shipments = dashboardData.Shipments;
            TotalShipments = dashboardData.TotalShipments;
            ActiveShipments = dashboardData.ActiveShipments;
            DeliveredShipments = dashboardData.DeliveredShipments;
            CustomsShipments = dashboardData.CustomsShipments;
            StatusSummaries = dashboardData.StatusSummaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading shipment dashboard data");
            Shipments = Array.Empty<ShipmentListItemDto>();
        }
    }

    public async Task<IActionResult> OnGetExportAsync(string format = "excel", CancellationToken cancellationToken = default)
    {
        try
        {
            var shipments = await _shipmentService.GetAllAsync(cancellationToken);
            var shipmentsList = shipments.Cast<object>().ToList();

            byte[] fileBytes;
            string contentType;
            string fileName;

            if (format.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                fileBytes = await _exportService.ExportShipmentsToCsvAsync(shipmentsList, cancellationToken);
                contentType = "text/csv";
                fileName = $"Sevkiyatlar_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            }
            else
            {
                fileBytes = await _exportService.ExportShipmentsToExcelAsync(shipmentsList, cancellationToken);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"Sevkiyatlar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting shipments");
            return RedirectToPage("./Index", new { error = "Export işlemi başarısız oldu." });
        }
    }

    public string GetStatusBadgeClass(ShipmentStatus status) =>
        status switch
        {
            ShipmentStatus.DeliveredToDealer => "status-chip--positive",
            ShipmentStatus.Cancelled => "status-chip--danger",
            ShipmentStatus.InCustoms => "status-chip--warning",
            ShipmentStatus.OnVessel or ShipmentStatus.OnTrain or ShipmentStatus.OnTruck => "status-chip--info",
            ShipmentStatus.InWarehouse => "status-chip--neutral",
            ShipmentStatus.ProductionStarted or ShipmentStatus.Ordered => "status-chip--secondary",
            _ => "status-chip--muted"
        };
}


