namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public sealed record DuplicatePatientCheckRequest(
    Guid FacilityId,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? NationalIdNumber,
    string? PassportNumber,
    string? PhoneNumber,
    Guid? ExistingPatientId = null);

