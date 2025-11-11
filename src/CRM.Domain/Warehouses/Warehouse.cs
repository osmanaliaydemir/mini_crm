using CRM.Domain.Abstractions;

namespace CRM.Domain.Warehouses;

public class Warehouse : Entity<Guid>, IAuditableEntity
{
    private readonly List<WarehouseUnloading> _unloadings = new();

    private Warehouse()
    {
    }

    public Warehouse(Guid id, string name, string? location, string? contactPerson, string? contactPhone)
    {
        Id = id;
        Name = name;
        Location = location;
        ContactPerson = contactPerson;
        ContactPhone = contactPhone;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<WarehouseUnloading> Unloadings => _unloadings.AsReadOnly();

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(string name, string? location, string? contactPerson, string? contactPhone, string? notes)
    {
        Name = name;
        Location = location;
        ContactPerson = contactPerson;
        ContactPhone = contactPhone;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }
}

