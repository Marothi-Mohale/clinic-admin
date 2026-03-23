namespace ClinicAdmin.Contracts.Auditing;

public sealed record AuditLogItemDto(
    Guid Id,
    DateTimeOffset OccurredAtUtc,
    string? ActorUsername,
    string Action,
    string EntityName,
    Guid? EntityId,
    string Details,
    string? BeforeSummary,
    string? AfterSummary,
    string? Metadata,
    string? Workstation,
    bool Succeeded);

