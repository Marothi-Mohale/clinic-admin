using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Files;
using ClinicAdmin.Domain.Patients;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicAdminDbContext).Assembly);
    }
}

