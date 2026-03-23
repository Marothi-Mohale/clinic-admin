namespace ClinicAdmin.Contracts.Patients;

public sealed record PatientProfileDto(
    Guid Id,
    string PatientNumber,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    string Sex,
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
    string? FileNumber,
    string? FileStatus,
    string? FileLocation,
    IReadOnlyCollection<PatientHistoryItemDto> History);
