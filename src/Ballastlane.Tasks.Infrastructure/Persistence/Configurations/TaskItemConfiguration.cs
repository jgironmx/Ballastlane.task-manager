using Ballastlane.Tasks.Domain.Tasks;
using Ballastlane.Tasks.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ballastlane.Tasks.Infrastructure.Persistence.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TaskItem.TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TaskItem.DescriptionMaxLength);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.DueDate);

        builder.Property(t => t.CreatedAtUtc)
            .IsRequired();

        builder.Property(t => t.UpdatedAtUtc);

        // Foreign key only — no navigation property, so Domain never references ApplicationUser
        // (see docs/decisions/ADR-003-identity-jwt.md). Deleting a user deletes their tasks.
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.OwnerId);
        builder.HasIndex(t => new { t.OwnerId, t.Status });
    }
}
