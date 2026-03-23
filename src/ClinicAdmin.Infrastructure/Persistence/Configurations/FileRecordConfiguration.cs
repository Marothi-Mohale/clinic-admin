using ClinicAdmin.Domain.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicAdmin.Infrastructure.Persistence.Configurations;

public sealed class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> builder)
    {
        builder.ToTable("FileRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CurrentLocation).HasMaxLength(100);
        builder.HasIndex(x => new { x.FacilityId, x.FileNumber }).IsUnique();
    }
}

