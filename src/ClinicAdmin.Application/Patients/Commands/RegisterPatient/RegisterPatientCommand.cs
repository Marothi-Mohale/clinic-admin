namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed record RegisterPatientCommand(
    Guid FacilityId,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string? NationalIdNumber,
    string? PhoneNumber);

