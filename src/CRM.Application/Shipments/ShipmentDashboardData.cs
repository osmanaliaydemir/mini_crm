using CRM.Domain.Enums;

namespace CRM.Application.Shipments;

public record ShipmentDashboardData(
    IReadOnlyList<ShipmentListItemDto> Shipments,
    int TotalShipments,
    int ActiveShipments,
    int DeliveredShipments,
    int CustomsShipments,
    IReadOnlyList<StatusSummary> StatusSummaries);

public record StatusSummary(ShipmentStatus Status, int Count);

