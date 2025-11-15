namespace CRM.Application.Warehouses;

public record WarehouseDto(
    Guid Id,
    string Name,
    string? Location,
    string? ContactPerson,
    string? ContactPhone,
    string? Notes,
    DateTime CreatedAt);

public record CreateWarehouseRequest(
    string Name,
    string? Location,
    string? ContactPerson,
    string? ContactPhone,
    string? Notes);

public record UpdateWarehouseRequest(
    Guid Id,
    string Name,
    string? Location,
    string? ContactPerson,
    string? ContactPhone,
    string? Notes);

