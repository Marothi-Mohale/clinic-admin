using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Auditing;
using ClinicAdmin.Domain.Files;
using ClinicAdmin.Domain.Patients;
using ClinicAdmin.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Infrastructure.Persistence;

public sealed class ClinicAdminDbContext : DbContext, IApplicationDbContext
{
    public ClinicAdminDbContext(DbContextOptions<ClinicAdminDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<FileRecord> Files => Set<FileRecord>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicAdminDbContext).Assembly);
    }
}
