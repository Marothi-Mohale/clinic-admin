namespace ClinicAdmin.Desktop.ViewModels;

public sealed class VisitHistoryItemViewModel
{
    public required Guid Id { get; init; }
    public required DateTimeOffset ArrivedAtUtc { get; init; }
    public required string ReasonForVisit { get; init; }
    public required string QueueStatus { get; init; }
    public required string State { get; init; }
    public string? Department { get; init; }
    public string? AssignedStaffMember { get; init; }
    public string Notes { get; init; } = string.Empty;
}

