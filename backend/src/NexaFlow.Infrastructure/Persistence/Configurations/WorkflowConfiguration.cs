using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaFlow.Domain.Entities;

namespace NexaFlow.Infrastructure.Persistence.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Description).HasMaxLength(2000);
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(w => w.TenantId);

        builder.HasOne(w => w.Tenant)
            .WithMany(t => t.Workflows)
            .HasForeignKey(w => w.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.CreatedByUser)
            .WithMany()
            .HasForeignKey(w => w.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
