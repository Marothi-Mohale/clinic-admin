namespace ClinicAdmin.Application.Abstractions;

public interface ISyncJournal
{
    Task EnqueueAsync(
        string eventType,
        string entityName,
        Guid entityId,
        Guid facilityId,
        string payload,
        CancellationToken cancellationToken = default);
}
