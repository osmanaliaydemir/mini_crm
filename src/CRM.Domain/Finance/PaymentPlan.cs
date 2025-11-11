using CRM.Domain.Abstractions;

namespace CRM.Domain.Finance;

public class PaymentPlan : Entity<Guid>, IAuditableEntity
{
    private readonly List<PaymentInstallment> _installments = new();

    private PaymentPlan()
    {
    }

    public PaymentPlan(
        Guid id,
        Guid customerId,
        Guid shipmentId,
        PaymentPlanType planType,
        decimal totalAmount,
        string currency,
        DateTime startDate,
        int periodicityWeeks)
    {
        Id = id;
        CustomerId = customerId;
        ShipmentId = shipmentId;
        PlanType = planType;
        TotalAmount = totalAmount;
        Currency = currency;
        StartDate = startDate;
        PeriodicityWeeks = periodicityWeeks;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public Guid ShipmentId { get; private set; }
    public PaymentPlanType PlanType { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public DateTime StartDate { get; private set; }
    public int PeriodicityWeeks { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<PaymentInstallment> Installments => _installments.AsReadOnly();

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(
        PaymentPlanType planType,
        decimal totalAmount,
        string currency,
        DateTime startDate,
        int periodicityWeeks,
        string? notes)
    {
        PlanType = planType;
        TotalAmount = totalAmount;
        Currency = currency;
        StartDate = startDate;
        PeriodicityWeeks = periodicityWeeks;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void SetInstallments(IEnumerable<PaymentInstallment> installments)
    {
        _installments.Clear();
        _installments.AddRange(installments);
    }
}

