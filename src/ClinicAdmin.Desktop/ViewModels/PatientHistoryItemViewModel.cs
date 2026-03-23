namespace ClinicAdmin.Desktop.ViewModels;

public sealed class PatientHistoryItemViewModel
{
    public required DateTimeOffset OccurredAtUtc { get; init; }
    public required string Action { get; init; }
    public required string Details { get; init; }
    public required bool Succeeded { get; init; }
}

