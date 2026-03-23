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
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IClock clock,
        IFacilityContext facilityContext,
        ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _clock = clock;
        _facilityContext = facilityContext;
        _logger = logger;
    }

    public async Task WriteAsync(string action, string entityName, Guid entityId, string details, CancellationToken cancellationToken = default)
    {
        var entry = new AuditEntry(
            _facilityContext.CurrentFacilityId,
            _currentUserService.Username,
            action,
            entityName,
            entityId,
            details,
            true,
            _clock.UtcNow);

        _dbContext.AuditEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Audit {Action} on {EntityName} ({EntityId}) {Details}", action, entityName, entityId, details);
    }

    public async Task WriteAuthenticationAsync(string attemptedUsername, bool succeeded, string details, CancellationToken cancellationToken = default)
    {
        var entry = new AuditEntry(
            _facilityContext.CurrentFacilityId,
            attemptedUsername,
            "AuthenticationAttempt",
            "AppUser",
            null,
            details,
            succeeded,
            _clock.UtcNow);

        _dbContext.AuditEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Authentication audit for {Username}: {Succeeded} {Details}", attemptedUsername, succeeded, details);
    }
}
