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
        builder.Property(x => x.PatientNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Sex).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.NationalIdNumber).HasMaxLength(20);
        builder.Property(x => x.PassportNumber).HasMaxLength(20);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(x => x.Line1).HasColumnName("AddressLine1").HasMaxLength(150);
            address.Property(x => x.Line2).HasColumnName("AddressLine2").HasMaxLength(150);
            address.Property(x => x.Suburb).HasColumnName("Suburb").HasMaxLength(100);
            address.Property(x => x.City).HasColumnName("City").HasMaxLength(100);
        });
        builder.OwnsOne(x => x.NextOfKin, nextOfKin =>
        {
            nextOfKin.Property(x => x.FullName).HasColumnName("NextOfKinName").HasMaxLength(150);
            nextOfKin.Property(x => x.Relationship).HasColumnName("NextOfKinRelationship").HasMaxLength(100);
            nextOfKin.Property(x => x.PhoneNumber).HasColumnName("NextOfKinPhoneNumber").HasMaxLength(20);
        });
        builder.HasIndex(x => new { x.FacilityId, x.PatientNumber }).IsUnique();
        builder.HasIndex(x => new { x.FacilityId, x.LastName, x.FirstName });
        builder.HasIndex(x => new { x.FacilityId, x.NationalIdNumber });
        builder.HasIndex(x => new { x.FacilityId, x.PassportNumber });
        builder.HasIndex(x => new { x.FacilityId, x.PhoneNumber });
    }
}
