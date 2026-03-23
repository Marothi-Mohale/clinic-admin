using ClinicAdmin.Contracts.Auditing;

namespace ClinicAdmin.Application.Auditing;

public interface IAuditLogQueryService
{
    Task<IReadOnlyCollection<AuditLogItemDto>> QueryAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default);
}

