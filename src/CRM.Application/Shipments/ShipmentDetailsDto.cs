using CRM.Domain.Enums;

namespace CRM.Application.Shipments;

public record ShipmentDetailsDto(Guid Id, string ReferenceNumber, string SupplierName, string? CustomerName, ShipmentStatus Status,
    DateTime ShipmentDate, DateTime? EstimatedArrival, string? LoadingPort, string? DischargePort, string? Notes, IReadOnlyList<ShipmentStageDto> Stages,
    CustomsInfoDto? Customs, IReadOnlyList<ShipmentItemDto> Items, IReadOnlyList<ShipmentTransportUnitDto> TransportUnits);

public record ShipmentStageDto(ShipmentStatus Status, DateTime StartedAt, DateTime? CompletedAt, string? Notes);

public record ShipmentItemDto(
    Guid VariantId,
    string VariantName,
    string? VariantSpecies,
    string? VariantGrade,
    string UnitOfMeasure,
    decimal Quantity,
    decimal Volume);

public record ShipmentTransportUnitDto(TransportMode Mode, string Identifier, int Count);

public record CustomsInfoDto(CustomsStatus Status, DateTime StartedAt, DateTime? CompletedAt, string? DocumentNumber, string? Notes);

