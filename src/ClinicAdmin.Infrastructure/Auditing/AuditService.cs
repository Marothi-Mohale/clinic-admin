using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Auditing;
using Microsoft.Extensions.Logging;

namespace ClinicAdmin.Infrastructure.Auditing;

public sealed class AuditService : IAuditService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;
    private readonly IFacilityContext _facilityContext;
    private readonly IWorkstationContext _workstationContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IClock clock,
        IFacilityContext facilityContext,
        IWorkstationContext workstationContext,
        ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _clock = clock;
        _facilityContext = facilityContext;
        _workstationContext = workstationContext;
        _logger = logger;
    }

    public Task WriteAsync(string action, string entityName, Guid entityId, string details, CancellationToken cancellationToken = default) =>
        WriteChangeAsync(action, entityName, entityId, details, cancellationToken: cancellationToken);

    public Task WriteAuthenticationAsync(string attemptedUsername, bool succeeded, string details, CancellationToken cancellationToken = default) =>
        WriteCoreAsync(
            attemptedUsername,
            "AuthenticationAttempt",
            "AppUser",
            null,
            details,
            null,
            null,
            "{\"category\":\"authentication\"}",
            succeeded,
            cancellationToken);

    public Task WriteChangeAsync(
        string action,
        string entityName,
        Guid? entityId,
        string details,
        string? beforeSummary = null,
        string? afterSummary = null,
        string? metadata = null,
        bool succeeded = true,
        CancellationToken cancellationToken = default) =>
        WriteCoreAsync(
            _currentUserService.Username,
            action,
            entityName,
            entityId,
            details,
            Sanitize(beforeSummary),
            Sanitize(afterSummary),
            metadata,
            succeeded,
            cancellationToken);

    private async Task WriteCoreAsync(
        string? actorUsername,
        string action,
        string entityName,
        Guid? entityId,
        string details,
        string? beforeSummary,
        string? afterSummary,
        string? metadata,
        bool succeeded,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEntry(
            _facilityContext.CurrentFacilityId,
            actorUsername,
            action,
            entityName,
            entityId,
            Sanitize(details),
            beforeSummary,
            afterSummary,
            metadata,
            _workstationContext.WorkstationName,
            succeeded,
            _clock.UtcNow);

        _dbContext.AuditEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Audit entry recorded {@AuditEvent}",
            new
            {
                entry.Action,
                entry.EntityName,
                entry.EntityId,
                entry.ActorUsername,
                entry.Succeeded,
                entry.Workstation,
                entry.OccurredAtUtc
            });
    }

    private static string? Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();
    }
}
