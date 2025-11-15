namespace CRM.Application.Customers;

public record CustomerDto(
    Guid Id,
    string Name,
    string? LegalName,
    string? TaxNumber,
    string? Email,
    string? Phone,
    string? Address,
    string? Segment,
    string? Notes,
    string? PrimaryContactName,
    string? PrimaryContactEmail,
    string? PrimaryContactPhone,
    string? PrimaryContactPosition,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? LastModifiedAt,
    string? LastModifiedBy);

public record CustomerListItemDto(
    Guid Id,
    string Name,
    string? LegalName,
    string? Segment,
    string? Email,
    string? Phone,
    string? Notes);

public record CreateCustomerRequest(
    string Name,
    string? LegalName,
    string? TaxNumber,
    string? Email,
    string? Phone,
    string? Address,
    string? Segment,
    string? Notes,
    string? PrimaryContactName,
    string? PrimaryContactEmail,
    string? PrimaryContactPhone,
    string? PrimaryContactPosition);

public record UpdateCustomerRequest(
    Guid Id,
    string Name,
    string? LegalName,
    string? TaxNumber,
    string? Email,
    string? Phone,
    string? Address,
    string? Segment,
    string? Notes,
    string? PrimaryContactName,
    string? PrimaryContactEmail,
    string? PrimaryContactPhone,
    string? PrimaryContactPosition);

