using ClinicAdmin.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicAdmin.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NationalIdNumber).HasMaxLength(20);
        builder.Property(x => x.PassportNumber).HasMaxLength(20);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.HasIndex(x => new { x.FacilityId, x.LastName, x.FirstName });
        builder.HasIndex(x => new { x.FacilityId, x.NationalIdNumber });
        builder.HasIndex(x => new { x.FacilityId, x.PassportNumber });
    }
}
