using CRM.Domain.Enums;

namespace CRM.Application.Shipments;

public record ShipmentDto(Guid Id, Guid SupplierId, string SupplierName, Guid? CustomerId,
    string? CustomerName, string ReferenceNumber, DateTime ShipmentDate, DateTime? EstimatedArrival,
    ShipmentStatus Status, string? LoadingPort, string? DischargePort, string? Notes, DateTime CreatedAt, byte[] RowVersion);

public record ShipmentListItemDto(Guid Id, string ReferenceNumber, string SupplierName, string CustomerName,
    ShipmentStatus Status, DateTime ShipmentDate, DateTime? EstimatedArrival, DateTime? LastStageUpdate, string? LastStageNotes);

public record ShipmentItemRequest(Guid VariantId, decimal Quantity, decimal Volume);

public record CreateShipmentRequest(Guid SupplierId, Guid? CustomerId, string ReferenceNumber, DateTime ShipmentDate, DateTime? EstimatedArrival,
    ShipmentStatus Status, string? LoadingPort, string? DischargePort, string? Notes,
    DateTime StageStartedAt, DateTime? StageCompletedAt, string? StageNotes,
    IReadOnlyList<ShipmentItemRequest>? Items = null);

public record UpdateShipmentRequest(Guid Id, Guid SupplierId, Guid? CustomerId, string ReferenceNumber,
    DateTime ShipmentDate, DateTime? EstimatedArrival, ShipmentStatus Status, string? LoadingPort, string? DischargePort,
    string? Notes, DateTime StageStartedAt, DateTime? StageCompletedAt, string? StageNotes, byte[] RowVersion,
    IReadOnlyList<ShipmentItemRequest>? Items = null);

