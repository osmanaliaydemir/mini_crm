using CRM.Domain.Abstractions;

namespace CRM.Domain.Finance;

public class PaymentInstallment : Entity<Guid>, IAuditableEntity
{
    private PaymentInstallment()
    {
    }

    public PaymentInstallment(Guid paymentPlanId, int installmentNumber, DateTime dueDate, decimal amount, string currency)
    {
        Id = Guid.NewGuid();
        PaymentPlanId = paymentPlanId;
        InstallmentNumber = installmentNumber;
        DueDate = dueDate;
        Amount = amount;
        Currency = currency;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid PaymentPlanId { get; private set; }
    public PaymentPlan? PaymentPlan { get; private set; }
    public int InstallmentNumber { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public decimal? PaidAmount { get; private set; }
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void RegisterPayment(DateTime paidAt, decimal paidAmount, string? notes)
    {
        PaidAt = paidAt;
        PaidAmount = paidAmount;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void UpdateSchedule(DateTime dueDate, decimal amount)
    {
        DueDate = dueDate;
        Amount = amount;
        LastModifiedAt = DateTime.UtcNow;
    }
}

