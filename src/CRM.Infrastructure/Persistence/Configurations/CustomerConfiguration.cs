using CRM.Domain.Customers;
using CRM.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LegalName)
            .HasMaxLength(200);

        builder.Property(x => x.TaxNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Address)
            .HasMaxLength(300);

        builder.Property(x => x.Segment)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasMany(x => x.Contacts)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<CustomerInteraction>()
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.Segment);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.Name, x.Segment }); // Composite index for common queries

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

public class CustomerContactConfiguration : IEntityTypeConfiguration<CustomerContact>
{
    public void Configure(EntityTypeBuilder<CustomerContact> builder)
    {
        builder.ToTable("CustomerContacts");

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Position)
            .HasMaxLength(100);

        // Performance indexes
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Email);
    }
}

public class CustomerInteractionConfiguration : IEntityTypeConfiguration<CustomerInteraction>
{
    public void Configure(EntityTypeBuilder<CustomerInteraction> builder)
    {
        builder.ToTable("CustomerInteractions");

        builder.Property(x => x.InteractionDate)
            .HasColumnType("datetime2");

        builder.Property(x => x.InteractionType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.RecordedBy)
            .HasMaxLength(100);

        // Performance indexes
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.InteractionDate);
        builder.HasIndex(x => x.InteractionType);
        builder.HasIndex(x => new { x.CustomerId, x.InteractionDate }); // Composite index for timeline queries

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

public class PaymentPlanConfiguration : IEntityTypeConfiguration<PaymentPlan>
{
    public void Configure(EntityTypeBuilder<PaymentPlan> builder)
    {
        builder.ToTable("PaymentPlans");

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("TRY");

        builder.Property(x => x.PeriodicityWeeks)
            .HasDefaultValue(1);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasMany(x => x.Installments)
            .WithOne(x => x.PaymentPlan)
            .HasForeignKey(x => x.PaymentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Performance indexes
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.ShipmentId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.CustomerId, x.CreatedAt }); // Composite index for customer payment plans
    }
}

public class PaymentInstallmentConfiguration : IEntityTypeConfiguration<PaymentInstallment>
{
    public void Configure(EntityTypeBuilder<PaymentInstallment> builder)
    {
        builder.ToTable("PaymentInstallments");

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("TRY");

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Performance indexes
        builder.HasIndex(x => x.PaymentPlanId);
        builder.HasIndex(x => x.DueDate);
    }
}

public class CashTransactionConfiguration : IEntityTypeConfiguration<CashTransaction>
{
    public void Configure(EntityTypeBuilder<CashTransaction> builder)
    {
        builder.ToTable("CashTransactions");

        builder.Property(x => x.TransactionDate)
            .HasColumnType("datetime2");

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("TRY");

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Category)
            .HasMaxLength(150);

        // Performance indexes
        builder.HasIndex(x => x.TransactionDate);
        builder.HasIndex(x => x.TransactionType);
        builder.HasIndex(x => x.RelatedCustomerId);
        builder.HasIndex(x => x.RelatedShipmentId);
        builder.HasIndex(x => new { x.TransactionDate, x.TransactionType }); // Composite index for filtering

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

