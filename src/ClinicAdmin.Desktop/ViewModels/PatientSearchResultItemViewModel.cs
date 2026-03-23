namespace ClinicAdmin.Desktop.ViewModels;

public sealed class PatientSearchResultItemViewModel
{
    public required Guid Id { get; init; }
    public required string PatientNumber { get; init; }
    public required string DisplayName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? NationalIdNumber { get; init; }
    public string? PassportNumber { get; init; }
    public string? PhoneNumber { get; init; }
    public string? FileNumber { get; init; }
    public string? FileStatus { get; init; }
    public string? FileLocation { get; init; }

    public string Identifier => NationalIdNumber ?? PassportNumber ?? "None";
}
