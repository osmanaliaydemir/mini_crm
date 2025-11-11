using CRM.Domain.Abstractions;
using CRM.Domain.Shipments;

namespace CRM.Domain.Warehouses;

public class WarehouseUnloading : Entity<Guid>, IAuditableEntity
{
    private WarehouseUnloading()
    {
    }

    public WarehouseUnloading(
        Guid warehouseId,
        Guid shipmentId,
        string truckPlate,
        DateTime unloadedAt,
        decimal unloadedVolume)
    {
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        ShipmentId = shipmentId;
        TruckPlate = truckPlate;
        UnloadedAt = unloadedAt;
        UnloadedVolume = unloadedVolume;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid WarehouseId { get; private set; }
    public Warehouse? Warehouse { get; private set; }
    public Guid ShipmentId { get; private set; }
    public Shipment? Shipment { get; private set; }
    public string TruckPlate { get; private set; } = string.Empty;
    public DateTime UnloadedAt { get; private set; }
    public decimal UnloadedVolume { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(DateTime unloadedAt, decimal unloadedVolume, string? notes)
    {
        UnloadedAt = unloadedAt;
        UnloadedVolume = unloadedVolume;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }
}

