using CRM.Domain.Abstractions;
using CRM.Domain.Enums;

namespace CRM.Domain.Shipments;

public class CustomsProcess : Entity<Guid>, IAuditableEntity
{
    private CustomsProcess()
    {
    }

    public CustomsProcess(Guid shipmentId, CustomsStatus status, DateTime startedAt, string? documentNumber = null)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        Status = status;
        StartedAt = startedAt;
        DocumentNumber = documentNumber;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ShipmentId { get; private set; }
    public Shipment? Shipment { get; private set; }
    public CustomsStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? DocumentNumber { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Update(CustomsStatus status, DateTime? completedAt, string? documentNumber, string? notes)
    {
        Status = status;
        CompletedAt = completedAt;
        DocumentNumber = documentNumber;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }
}

