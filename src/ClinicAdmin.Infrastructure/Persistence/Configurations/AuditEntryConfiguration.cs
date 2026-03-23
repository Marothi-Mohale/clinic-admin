using ClinicAdmin.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicAdmin.Infrastructure.Persistence.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ActorUsername).HasMaxLength(50);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.BeforeSummary).HasMaxLength(2000);
        builder.Property(x => x.AfterSummary).HasMaxLength(2000);
        builder.Property(x => x.Metadata).HasMaxLength(2000);
        builder.Property(x => x.Workstation).HasMaxLength(100);
        builder.Property(x => x.Succeeded).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => x.OccurredAtUtc);
        builder.HasIndex(x => new { x.FacilityId, x.Action });
        builder.HasIndex(x => new { x.FacilityId, x.EntityName });
    }
}
