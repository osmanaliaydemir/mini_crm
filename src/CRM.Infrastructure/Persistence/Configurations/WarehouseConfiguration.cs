using CRM.Domain.Warehouses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.Property(x => x.ContactPerson)
            .HasMaxLength(150);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(50);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.HasMany(x => x.Unloadings)
            .WithOne(x => x.Warehouse)
            .HasForeignKey(x => x.WarehouseId);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

public class WarehouseUnloadingConfiguration : IEntityTypeConfiguration<WarehouseUnloading>
{
    public void Configure(EntityTypeBuilder<WarehouseUnloading> builder)
    {
        builder.ToTable("WarehouseUnloadings");

        builder.Property(x => x.TruckPlate)
            .HasMaxLength(50);

        builder.Property(x => x.UnloadedVolume)
            .HasPrecision(18, 3);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

