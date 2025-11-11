using CRM.Domain.Abstractions;
using CRM.Domain.Enums;

namespace CRM.Domain.Shipments;

public class ShipmentStage : Entity<Guid>, IAuditableEntity
{
    private ShipmentStage()
    {
    }

    internal ShipmentStage(Guid shipmentId, ShipmentStatus status, DateTime startedAt, string? notes)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        Status = status;
        StartedAt = startedAt;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ShipmentId { get; private set; }
    public Shipment? Shipment { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(DateTime startedAt, DateTime? completedAt, string? notes)
    {
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ShipmentStatus status)
    {
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
    }
}


