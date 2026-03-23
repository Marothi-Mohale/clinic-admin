using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Auditing;
using ClinicAdmin.Domain.Files;
using ClinicAdmin.Domain.Patients;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Domain.Visits;
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
    public DbSet<PatientVisit> Visits => Set<PatientVisit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicAdminDbContext).Assembly);
    }
}
