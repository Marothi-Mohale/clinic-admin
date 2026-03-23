using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Contracts.Auditing;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Auditing;

public sealed class AuditLogQueryService : IAuditLogQueryService
{
    private readonly IApplicationDbContext _dbContext;

    public AuditLogQueryService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AuditLogItemDto>> QueryAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(query.Take, 1, 250);
        var searchTerm = string.IsNullOrWhiteSpace(query.SearchTerm) ? null : query.SearchTerm.Trim().ToUpperInvariant();
        var actor = string.IsNullOrWhiteSpace(query.ActorUsername) ? null : query.ActorUsername.Trim().ToUpperInvariant();
        var action = string.IsNullOrWhiteSpace(query.Action) ? null : query.Action.Trim().ToUpperInvariant();
        var entityName = string.IsNullOrWhiteSpace(query.EntityName) ? null : query.EntityName.Trim().ToUpperInvariant();

        var auditEntries = _dbContext.AuditEntries
            .AsNoTracking()
            .Where(x => x.FacilityId == query.FacilityId);

        if (query.FromUtc is not null)
        {
            auditEntries = auditEntries.Where(x => x.OccurredAtUtc >= query.FromUtc.Value);
        }

        if (query.ToUtc is not null)
        {
            auditEntries = auditEntries.Where(x => x.OccurredAtUtc <= query.ToUtc.Value);
        }

        if (actor is not null)
        {
            auditEntries = auditEntries.Where(x => x.ActorUsername != null && x.ActorUsername == actor);
        }

        if (action is not null)
        {
            auditEntries = auditEntries.Where(x => x.Action.ToUpper() == action);
        }

        if (entityName is not null)
        {
            auditEntries = auditEntries.Where(x => x.EntityName.ToUpper() == entityName);
        }

        if (searchTerm is not null)
        {
            auditEntries = auditEntries.Where(x =>
                x.Action.ToUpper().Contains(searchTerm) ||
                x.EntityName.ToUpper().Contains(searchTerm) ||
                x.Details.ToUpper().Contains(searchTerm) ||
                (x.ActorUsername != null && x.ActorUsername.ToUpper().Contains(searchTerm)));
        }

        return await auditEntries
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .Select(x => new AuditLogItemDto(
                x.Id,
                x.OccurredAtUtc,
                x.ActorUsername,
                x.Action,
                x.EntityName,
                x.EntityId,
                x.Details,
                x.BeforeSummary,
                x.AfterSummary,
                x.Metadata,
                x.Workstation,
                x.Succeeded))
            .ToListAsync(cancellationToken);
    }
}
