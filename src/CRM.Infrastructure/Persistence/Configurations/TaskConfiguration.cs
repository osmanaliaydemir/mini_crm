using CRM.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TaskDb>
{
    public void Configure(EntityTypeBuilder<TaskDb> builder)
    {
        builder.ToTable("TaskDb");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Tasks.TaskStatus.Pending)
            .HasSentinel((Domain.Tasks.TaskStatus)(-1)); // Sentinel value that will never be used

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(TaskPriority.Medium)
            .HasSentinel(TaskPriority.Low);

        builder.Property(x => x.DueDate)
            .HasColumnType("datetime2");

        builder.Property(x => x.AssignedToUserId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(x => x.RelatedEntityId)
            .HasColumnType("uniqueidentifier");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedAt)
            .HasColumnType("datetime2");

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();
    }
}

