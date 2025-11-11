using CRM.Domain.Abstractions;
using System.Linq;
using CRM.Domain.Customers;
using CRM.Domain.Enums;
using CRM.Domain.Products;
using CRM.Domain.Suppliers;

namespace CRM.Domain.Shipments;

public class Shipment : Entity<Guid>, IAuditableEntity
{
    private readonly List<ShipmentItem> _items = new();
    private readonly List<ShipmentTransportUnit> _transportUnits = new();
    private readonly List<ShipmentStage> _stages = new();

    private Shipment()
    {
    }

    public Shipment(
        Guid id,
        Guid supplierId,
        string referenceNumber,
        DateTime shipmentDate,
        ShipmentStatus status,
        Guid? customerId = null)
    {
        Id = id;
        SupplierId = supplierId;
        CustomerId = customerId;
        ReferenceNumber = referenceNumber;
        ShipmentDate = shipmentDate;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public DateTime ShipmentDate { get; private set; }
    public DateTime? EstimatedArrival { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string? LoadingPort { get; private set; }
    public string? DischargePort { get; private set; }
    public string? Notes { get; private set; }

    public CustomsProcess? CustomsProcess { get; private set; }

    public IReadOnlyCollection<ShipmentItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<ShipmentTransportUnit> TransportUnits => _transportUnits.AsReadOnly();
    public IReadOnlyCollection<ShipmentStage> Stages => _stages.AsReadOnly();

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(
        DateTime shipmentDate,
        DateTime? estimatedArrival,
        ShipmentStatus status,
        string? loadingPort,
        string? dischargePort,
        string? notes,
        Guid? customerId = null)
    {
        ShipmentDate = shipmentDate;
        EstimatedArrival = estimatedArrival;
        Status = status;
        LoadingPort = loadingPort;
        DischargePort = dischargePort;
        Notes = notes;
        CustomerId = customerId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid variantId, decimal quantity, decimal volume)
    {
        _items.Add(new ShipmentItem(Id, variantId, quantity, volume));
    }

    public void ClearItems() => _items.Clear();

    public void AddTransportUnit(TransportMode mode, string identifier, int count)
    {
        _transportUnits.Add(new ShipmentTransportUnit(Id, mode, identifier, count));
    }

    public void ClearTransportUnits() => _transportUnits.Clear();

    public void SetCustomsProcess(CustomsProcess customsProcess)
    {
        CustomsProcess = customsProcess;
    }

    public ShipmentStage SetOrUpdateStage(
        ShipmentStatus status,
        DateTime startedAt,
        DateTime? completedAt,
        string? notes)
    {
        var stage = _stages.FirstOrDefault(s => s.Status == status);
        if (stage is null)
        {
            stage = new ShipmentStage(Id, status, startedAt, notes);
            _stages.Add(stage);
        }

        stage.Update(startedAt, completedAt, notes);
        stage.UpdateStatus(status);
        Status = status;
        LastModifiedAt = DateTime.UtcNow;
        return stage;
    }

    public void RemoveStage(ShipmentStatus status)
    {
        var stage = _stages.FirstOrDefault(s => s.Status == status);
        if (stage is not null)
        {
            _stages.Remove(stage);
        }
    }

    public void ReassignSupplier(Guid supplierId)
    {
        SupplierId = supplierId;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ReassignCustomer(Guid? customerId)
    {
        CustomerId = customerId;
        LastModifiedAt = DateTime.UtcNow;
    }
}

public class ShipmentItem : Entity<Guid>
{
    private ShipmentItem()
    {
    }

    internal ShipmentItem(Guid shipmentId, Guid variantId, decimal quantity, decimal volume)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        VariantId = variantId;
        Quantity = quantity;
        Volume = volume;
    }

    public Guid ShipmentId { get; private set; }
    public Shipment? Shipment { get; private set; }
    public Guid VariantId { get; private set; }
    public LumberVariant? Variant { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Volume { get; private set; }
}

public class ShipmentTransportUnit : Entity<Guid>
{
    private ShipmentTransportUnit()
    {
    }

    internal ShipmentTransportUnit(Guid shipmentId, TransportMode mode, string identifier, int count)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        Mode = mode;
        Identifier = identifier;
        Count = count;
    }

    public Guid ShipmentId { get; private set; }
    public Shipment? Shipment { get; private set; }
    public TransportMode Mode { get; private set; }
    public string Identifier { get; private set; } = string.Empty;
    public int Count { get; private set; }
}

