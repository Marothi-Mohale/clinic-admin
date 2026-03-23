using ClinicAdmin.Domain.Visits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicAdmin.Infrastructure.Persistence.Configurations;

public sealed class PatientVisitConfiguration : IEntityTypeConfiguration<PatientVisit>
{
    public void Configure(EntityTypeBuilder<PatientVisit> builder)
    {
        builder.ToTable("Visits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReasonForVisit).HasMaxLength(200).IsRequired();
        builder.Property(x => x.QueueStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.State).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(100);
        builder.Property(x => x.AssignedStaffMember).HasMaxLength(120);
        builder.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ArrivedAtUtc).IsRequired();
        builder.Property(x => x.LastUpdatedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.FacilityId, x.PatientId, x.ArrivedAtUtc });
        builder.HasIndex(x => new { x.FacilityId, x.State, x.QueueStatus });
    }
}
