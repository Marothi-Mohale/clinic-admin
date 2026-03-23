namespace ClinicAdmin.Desktop.ViewModels;

public sealed class DuplicatePatientWarningItemViewModel
{
    public required Guid PatientId { get; init; }
    public required string DisplayName { get; init; }
    public required string PatientNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? NationalIdNumber { get; init; }
    public string? PassportNumber { get; init; }
    public string? PhoneNumber { get; init; }
    public required string Strength { get; init; }
    public required string Recommendation { get; init; }
    public required int Score { get; init; }
    public required string ReasonSummary { get; init; }
}

