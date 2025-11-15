using CRM.Domain.Abstractions;

namespace CRM.Domain.Customers;

public class Customer : Entity<Guid>, IAuditableEntity
{
    private readonly List<CustomerContact> _contacts = new();

    private Customer()
    {
    }

    public Customer(Guid id, string name, string? legalName, string? taxNumber, string? email,
        string? phone, string? address, string? segment, string? notes)
    {
        Id = id;
        Name = name;
        LegalName = legalName;
        TaxNumber = taxNumber;
        Email = email;
        Phone = phone;
        Address = address;
        Segment = segment;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? Segment { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<CustomerContact> Contacts => _contacts.AsReadOnly();

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(string name, string? legalName, string? taxNumber, string? email,
        string? phone, string? address, string? segment, string? notes)
    {
        Name = name;
        LegalName = legalName;
        TaxNumber = taxNumber;
        Email = email;
        Phone = phone;
        Address = address;
        Segment = segment;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddContact(string fullName, string? email, string? phone, string? position)
    {
        _contacts.Add(new CustomerContact(Id, fullName, email, phone, position));
    }

    public void ClearContacts() => _contacts.Clear();
}

