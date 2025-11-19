using CRM.Domain.Common.ValueObjects;

namespace CRM.Application.Products;

public record LumberVariantDto(
    Guid Id,
    string Name,
    string? Species,
    string? Grade,
    Measurement? StandardVolume,
    string UnitOfMeasure,
    string? Notes,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? LastModifiedAt,
    string? LastModifiedBy,
    byte[] RowVersion);

public record LumberVariantListItemDto(
    Guid Id,
    string Name,
    string? Species,
    string? Grade,
    string UnitOfMeasure);

public record CreateLumberVariantRequest(
    string Name,
    string? Species,
    string? Grade,
    Measurement? StandardVolume,
    string UnitOfMeasure,
    string? Notes);

public record UpdateLumberVariantRequest(
    Guid Id,
    string Name,
    string? Species,
    string? Grade,
    Measurement? StandardVolume,
    string UnitOfMeasure,
    string? Notes,
    byte[] RowVersion);

