using ClinicAdmin.Domain.Common;

namespace ClinicAdmin.Domain.Patients;

public sealed class Patient : Entity
{
    private Patient()
    {
        PatientNumber = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Address = new Address(null, null, null, null);
        NextOfKin = new NextOfKin(null, null, null);
    }

    public string PatientNumber { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public Sex Sex { get; private set; }
    public string? NationalIdNumber { get; private set; }
    public string? PassportNumber { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Address Address { get; private set; }
    public NextOfKin NextOfKin { get; private set; }
    public Guid FacilityId { get; private set; }

    public Patient(
        Guid facilityId,
        string patientNumber,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth,
        Sex sex,
        string? nationalIdNumber,
        string? passportNumber,
        string? phoneNumber,
        Address address,
        NextOfKin nextOfKin)
    {
        FacilityId = facilityId;
        PatientNumber = GuardRequired(patientNumber, nameof(patientNumber));
        FirstName = GuardRequired(firstName, nameof(firstName));
        LastName = GuardRequired(lastName, nameof(lastName));
        DateOfBirth = dateOfBirth;
        Sex = sex;
        NationalIdNumber = Normalize(nationalIdNumber);
        PassportNumber = Normalize(passportNumber);
        PhoneNumber = Normalize(phoneNumber);
        Address = address ?? throw new ArgumentNullException(nameof(address));
        NextOfKin = nextOfKin ?? throw new ArgumentNullException(nameof(nextOfKin));
    }

    public void UpdateDemographics(
        string patientNumber,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth,
        Sex sex,
        string? nationalIdNumber,
        string? passportNumber,
        string? phoneNumber,
        Address address,
        NextOfKin nextOfKin)
    {
        PatientNumber = GuardRequired(patientNumber, nameof(patientNumber));
        FirstName = GuardRequired(firstName, nameof(firstName));
        LastName = GuardRequired(lastName, nameof(lastName));
        DateOfBirth = dateOfBirth;
        Sex = sex;
        NationalIdNumber = Normalize(nationalIdNumber);
        PassportNumber = Normalize(passportNumber);
        PhoneNumber = Normalize(phoneNumber);
        Address = address ?? throw new ArgumentNullException(nameof(address));
        NextOfKin = nextOfKin ?? throw new ArgumentNullException(nameof(nextOfKin));
    }

    private static string GuardRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.", fieldName);
        }

        return value.Trim();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
