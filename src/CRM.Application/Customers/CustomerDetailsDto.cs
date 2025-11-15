namespace CRM.Application.Customers;

public record CustomerDetailsDto(CustomerDto Customer, IReadOnlyList<CustomerContactDto> Contacts, IReadOnlyList<CustomerInteractionDto> Interactions);

public record CustomerContactDto(Guid Id, Guid CustomerId, string FullName, string? Email, string? Phone, string? Position);

public record CustomerInteractionDto(Guid Id, Guid CustomerId, DateTime InteractionDate,
    string InteractionType, string? Subject, string? Notes, string? RecordedBy, DateTime CreatedAt);

