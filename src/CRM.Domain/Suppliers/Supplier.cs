using CRM.Domain.Abstractions;

namespace CRM.Domain.Suppliers;

public class Supplier : Entity<Guid>, IAuditableEntity
{
    private Supplier()
    {
    }

    public Supplier(Guid id, string name, string? country, string? taxNumber, string? contactEmail, string? contactPhone, string? addressLine)
    {
        Id = id;
        Name = name;
        Country = country;
        TaxNumber = taxNumber;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        AddressLine = addressLine;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Country { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? AddressLine { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(string name, string? country, string? taxNumber, string? contactEmail, string? contactPhone, string? addressLine, string? notes)
    {
        Name = name;
        Country = country;
        TaxNumber = taxNumber;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        AddressLine = addressLine;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }
}

