namespace ClinicAdmin.Domain.Patients;

public sealed class NextOfKin
{
    private NextOfKin()
    {
    }

    public string? FullName { get; private set; }
    public string? Relationship { get; private set; }
    public string? PhoneNumber { get; private set; }

    public NextOfKin(string? fullName, string? relationship, string? phoneNumber)
    {
        FullName = Normalize(fullName);
        Relationship = Normalize(relationship);
        PhoneNumber = Normalize(phoneNumber);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
