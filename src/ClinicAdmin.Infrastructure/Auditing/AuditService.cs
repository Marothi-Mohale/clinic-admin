using ClinicAdmin.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace ClinicAdmin.Infrastructure.Auditing;

public sealed class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public Task WriteAsync(string action, string entityName, Guid entityId, string details, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Audit {Action} on {EntityName} ({EntityId}) {Details}", action, entityName, entityId, details);
        return Task.CompletedTask;
    }
}

