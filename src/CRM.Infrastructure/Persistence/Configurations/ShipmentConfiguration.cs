using CRM.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");

        builder.Property(x => x.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LoadingPort)
            .HasMaxLength(150);

        builder.Property(x => x.DischargePort)
            .HasMaxLength(150);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasOne(x => x.CustomsProcess)
            .WithOne(x => x.Shipment!)
            .HasForeignKey<CustomsProcess>(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Shipment)
            .HasForeignKey(x => x.ShipmentId);

        builder.HasMany(x => x.TransportUnits)
            .WithOne(x => x.Shipment)
            .HasForeignKey(x => x.ShipmentId);

        builder.HasMany(x => x.Stages)
            .WithOne(x => x.Shipment)
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.ToTable("ShipmentItems");

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 3);

        builder.Property(x => x.Volume)
            .HasPrecision(18, 3);
    }
}

public class ShipmentTransportUnitConfiguration : IEntityTypeConfiguration<ShipmentTransportUnit>
{
    public void Configure(EntityTypeBuilder<ShipmentTransportUnit> builder)
    {
        builder.ToTable("ShipmentTransportUnits");

        builder.Property(x => x.Identifier)
            .HasMaxLength(100);
    }
}

public class ShipmentStageConfiguration : IEntityTypeConfiguration<ShipmentStage>
{
    public void Configure(EntityTypeBuilder<ShipmentStage> builder)
    {
        builder.ToTable("ShipmentStages");

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.StartedAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.CompletedAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.Notes)
            .HasMaxLength(500);
    }
}

public class CustomsProcessConfiguration : IEntityTypeConfiguration<CustomsProcess>
{
    public void Configure(EntityTypeBuilder<CustomsProcess> builder)
    {
        builder.ToTable("CustomsProcesses");

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(100);
    }
}

