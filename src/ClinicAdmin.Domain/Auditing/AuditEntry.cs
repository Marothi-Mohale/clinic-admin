using ClinicAdmin.Domain.Common;

namespace ClinicAdmin.Domain.Auditing;

public sealed class AuditEntry : Entity
{
    public Guid? FacilityId { get; private set; }
    public string? ActorUsername { get; private set; }
    public string Action { get; private set; }
    public string EntityName { get; private set; }
    public Guid? EntityId { get; private set; }
    public string Details { get; private set; }
    public bool Succeeded { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public AuditEntry(
        Guid? facilityId,
        string? actorUsername,
        string action,
        string entityName,
        Guid? entityId,
        string details,
        bool succeeded,
        DateTimeOffset occurredAtUtc)
    {
        FacilityId = facilityId;
        ActorUsername = string.IsNullOrWhiteSpace(actorUsername) ? null : actorUsername.Trim().ToUpperInvariant();
        Action = GuardRequired(action, nameof(action));
        EntityName = GuardRequired(entityName, nameof(entityName));
        EntityId = entityId;
        Details = GuardRequired(details, nameof(details));
        Succeeded = succeeded;
        OccurredAtUtc = occurredAtUtc;
    }

    private static string GuardRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.", fieldName);
        }

        return value.Trim();
    }
}
