using ClinicAdmin.Domain.Files;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Domain.Auditing;
using ClinicAdmin.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<FileRecord> Files { get; }
    DbSet<AppUser> Users { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
