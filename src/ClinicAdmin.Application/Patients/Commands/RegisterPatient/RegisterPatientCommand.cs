using ClinicAdmin.Domain.Patients;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed record RegisterPatientCommand(
    Guid FacilityId,
    string PatientNumber,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    Sex Sex,
    string? NationalIdNumber,
    string? PassportNumber,
    string? PhoneNumber,
    string? AddressLine1,
    string? AddressLine2,
    string? Suburb,
    string? City,
    string? NextOfKinName,
    string? NextOfKinRelationship,
    string? NextOfKinPhoneNumber,
    bool DuplicateWarningAcknowledged = false);
