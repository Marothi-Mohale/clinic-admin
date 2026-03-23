using ClinicAdmin.Domain.Common;

namespace ClinicAdmin.Domain.Patients;

public sealed class Patient : Entity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? NationalIdNumber { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Guid FacilityId { get; private set; }

    public Patient(
        Guid facilityId,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth,
        string? nationalIdNumber,
        string? phoneNumber)
    {
        FacilityId = facilityId;
        FirstName = GuardRequired(firstName, nameof(firstName));
        LastName = GuardRequired(lastName, nameof(lastName));
        DateOfBirth = dateOfBirth;
        NationalIdNumber = Normalize(nationalIdNumber);
        PhoneNumber = Normalize(phoneNumber);
    }

    public void UpdateDemographics(string firstName, string lastName, DateOnly? dateOfBirth, string? nationalIdNumber, string? phoneNumber)
    {
        FirstName = GuardRequired(firstName, nameof(firstName));
        LastName = GuardRequired(lastName, nameof(lastName));
        DateOfBirth = dateOfBirth;
        NationalIdNumber = Normalize(nationalIdNumber);
        PhoneNumber = Normalize(phoneNumber);
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

