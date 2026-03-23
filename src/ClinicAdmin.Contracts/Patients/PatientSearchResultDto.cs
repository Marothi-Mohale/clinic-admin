namespace ClinicAdmin.Contracts.Patients;

public sealed record PatientSearchResultDto(
    Guid Id,
    string PatientNumber,
    string DisplayName,
    DateOnly? DateOfBirth,
    string? NationalIdNumber,
    string? PassportNumber,
    string? PhoneNumber,
    string? FileNumber,
    string? FileStatus,
    string? FileLocation);
