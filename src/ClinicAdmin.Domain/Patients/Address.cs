namespace ClinicAdmin.Domain.Patients;

public sealed class Address
{
    private Address()
    {
    }

    public string? Line1 { get; private set; }
    public string? Line2 { get; private set; }
    public string? Suburb { get; private set; }
    public string? City { get; private set; }

    public Address(string? line1, string? line2, string? suburb, string? city)
    {
        Line1 = Normalize(line1);
        Line2 = Normalize(line2);
        Suburb = Normalize(suburb);
        City = Normalize(city);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
