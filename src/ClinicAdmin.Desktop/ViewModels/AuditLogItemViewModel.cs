namespace ClinicAdmin.Desktop.ViewModels;

public sealed class AuditLogItemViewModel
{
    public required Guid Id { get; init; }
    public required DateTimeOffset OccurredAtUtc { get; init; }
    public string? ActorUsername { get; init; }
    public required string Action { get; init; }
    public required string EntityName { get; init; }
    public Guid? EntityId { get; init; }
    public required string Details { get; init; }
    public string? BeforeSummary { get; init; }
    public string? AfterSummary { get; init; }
    public string? Metadata { get; init; }
    public string? Workstation { get; init; }
    public required bool Succeeded { get; init; }
}

