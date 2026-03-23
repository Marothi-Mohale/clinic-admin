namespace ClinicAdmin.Contracts.Patients;

public sealed record PatientSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? NationalIdNumber,
    string? PhoneNumber,
    string? FileNumber,
    string? FileStatus);

