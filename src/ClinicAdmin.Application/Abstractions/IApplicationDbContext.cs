using ClinicAdmin.Domain.Files;
using ClinicAdmin.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<FileRecord> Files { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

