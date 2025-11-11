using CRM.Domain.Abstractions;

namespace CRM.Domain.Customers;

public class CustomerContact : Entity<Guid>
{
    private CustomerContact()
    {
    }

    internal CustomerContact(Guid customerId, string fullName, string? email, string? phone, string? position)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        FullName = fullName;
        Email = email;
        Phone = phone;
        Position = position;
    }

    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Position { get; private set; }
}

