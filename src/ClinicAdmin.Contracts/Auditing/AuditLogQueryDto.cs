namespace ClinicAdmin.Contracts.Auditing;

public sealed record AuditLogQueryDto(
    Guid FacilityId,
    string? SearchTerm,
    string? Action,
    string? EntityName,
    string? ActorUsername,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int Take = 100);
