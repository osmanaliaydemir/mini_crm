using CRM.Domain.Abstractions;

namespace CRM.Domain.Finance;

public class CashTransaction : Entity<Guid>, IAuditableEntity
{
    private CashTransaction()
    {
    }

    public CashTransaction(
        Guid id,
        DateTime transactionDate,
        CashTransactionType transactionType,
        decimal amount,
        string currency,
        string? description,
        string? category,
        Guid? relatedCustomerId,
        Guid? relatedShipmentId)
    {
        Id = id;
        TransactionDate = transactionDate;
        TransactionType = transactionType;
        Amount = amount;
        Currency = currency;
        Description = description;
        Category = category;
        RelatedCustomerId = relatedCustomerId;
        RelatedShipmentId = relatedShipmentId;
        CreatedAt = DateTime.UtcNow;
    }

    public DateTime TransactionDate { get; private set; }
    public CashTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "TRY";
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public Guid? RelatedCustomerId { get; private set; }
    public Guid? RelatedShipmentId { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }

    public void Update(
        DateTime transactionDate,
        CashTransactionType transactionType,
        decimal amount,
        string currency,
        string? description,
        string? category,
        Guid? relatedCustomerId,
        Guid? relatedShipmentId)
    {
        TransactionDate = transactionDate;
        TransactionType = transactionType;
        Amount = amount;
        Currency = currency;
        Description = description;
        Category = category;
        RelatedCustomerId = relatedCustomerId;
        RelatedShipmentId = relatedShipmentId;
        LastModifiedAt = DateTime.UtcNow;
    }
}

