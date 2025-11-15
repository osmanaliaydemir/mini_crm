namespace CRM.Application.Suppliers;

public record SupplierDto(Guid Id, string Name, string? Country, string? TaxNumber,
    string? ContactEmail, string? ContactPhone, string? AddressLine, string? Notes, DateTime CreatedAt);

public record CreateSupplierRequest(string Name, string? Country, string? TaxNumber,
    string? ContactEmail, string? ContactPhone, string? AddressLine, string? Notes);

public record UpdateSupplierRequest(Guid Id, string Name, string? Country,
    string? TaxNumber, string? ContactEmail, string? ContactPhone, string? AddressLine, string? Notes);

