using CRM.Domain.Abstractions;

namespace CRM.Domain.Customers;

public class CustomerInteraction : Entity<Guid>, IAuditableEntity
{
    private CustomerInteraction()
    {
    }

    public CustomerInteraction(Guid customerId, DateTime interactionDate, string interactionType,
        string? subject, string? notes, string? recordedBy)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        InteractionDate = interactionDate;
        InteractionType = interactionType;
        Subject = subject;
        Notes = notes;
        RecordedBy = recordedBy;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public DateTime InteractionDate { get; private set; }
    public string InteractionType { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public string? Notes { get; private set; }
    public string? RecordedBy { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Update(DateTime interactionDate, string interactionType, string? subject, string? notes, string? recordedBy)
    {
        InteractionDate = interactionDate;
        InteractionType = interactionType;
        Subject = subject;
        Notes = notes;
        RecordedBy = recordedBy;
        LastModifiedAt = DateTime.UtcNow;
    }
}

