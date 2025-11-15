using CRM.Application.Shipments;
using CRM.Domain.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CRM.Web.Pages.Shipments;

public class IndexModel : PageModel
{
    private readonly IShipmentService _shipmentService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IShipmentService shipmentService, ILogger<IndexModel> logger)
    {
        _shipmentService = shipmentService;
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


