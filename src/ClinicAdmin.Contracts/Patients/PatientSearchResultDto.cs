namespace ClinicAdmin.Contracts.Patients;

public sealed record PatientSearchResultDto(
    Guid Id,
    string DisplayName,
    DateOnly? DateOfBirth,
    string? NationalIdNumber,
    string? FileNumber);
