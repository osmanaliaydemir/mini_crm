using CRM.Domain.Common.ValueObjects;
using CRM.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class LumberVariantConfiguration : IEntityTypeConfiguration<LumberVariant>
{
    public void Configure(EntityTypeBuilder<LumberVariant> builder)
    {
        builder.ToTable("LumberVariants");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Species)
            .HasMaxLength(100);

        builder.Property(x => x.Grade)
            .HasMaxLength(50);

        builder.Property(x => x.UnitOfMeasure)
            .HasMaxLength(20)
            .HasDefaultValue("m3");

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.OwnsOne(x => x.StandardVolume, mv =>
        {
            mv.Property(m => m.Amount)
                .HasColumnName("StandardVolumeAmount")
                .HasPrecision(18, 4);

            mv.Property(m => m.Unit)
                .HasColumnName("StandardVolumeUnit")
                .HasMaxLength(20);
        });

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

