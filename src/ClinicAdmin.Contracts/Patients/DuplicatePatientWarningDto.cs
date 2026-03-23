namespace ClinicAdmin.Contracts.Patients;

public sealed record DuplicatePatientWarningDto(
    Guid PatientId,
    int Score,
    string Strength,
    string Recommendation,
    string DisplayName,
    DateOnly? DateOfBirth,
    string? PatientNumber,
    string? NationalIdNumber,
    string? PassportNumber,
    string? PhoneNumber,
    IReadOnlyCollection<string> Reasons);
