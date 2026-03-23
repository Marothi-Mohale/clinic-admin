using ClinicAdmin.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace ClinicAdmin.Infrastructure.Sync;

public sealed class SyncJournalService : ISyncJournal
{
    private readonly ILogger<SyncJournalService> _logger;

    public SyncJournalService(ILogger<SyncJournalService> logger)
    {
        _logger = logger;
    }

    public Task EnqueueAsync(
        string eventType,
        string entityName,
        Guid entityId,
        Guid facilityId,
        string payload,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sync journal event {EventType} for {EntityName} ({EntityId}) at facility {FacilityId} payload {Payload}",
            eventType,
            entityName,
            entityId,
            facilityId,
            payload);

        return Task.CompletedTask;
    }
}
