namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public sealed record DuplicatePatientCandidate(
    Guid PatientId,
    Guid FacilityId,
    string PatientNumber,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? NationalIdNumber,
    string? PassportNumber,
    string? PhoneNumber);
