namespace CRM.Application.Warehouses;

public record WarehouseDetailsDto(WarehouseDto Warehouse, IReadOnlyList<WarehouseUnloadingDto> Unloadings,
    IReadOnlyList<ShipmentOptionDto> ShipmentOptions);

public record WarehouseUnloadingDto(Guid Id, Guid WarehouseId, Guid ShipmentId, string TruckPlate,
    DateTime UnloadedAt, decimal UnloadedVolume, string? Notes, DateTime CreatedAt);

public record ShipmentOptionDto(Guid Id, string DisplayText);

public record AddUnloadingRequest(Guid WarehouseId, Guid ShipmentId, string TruckPlate, DateTime UnloadedAt, decimal UnloadedVolume, string? Notes);

