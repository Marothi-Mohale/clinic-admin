using ClinicAdmin.Domain.Patients;

namespace ClinicAdmin.Domain.Tests.Patients;

public sealed class PatientTests
{
    [Fact]
    public void Constructor_ShouldTrimNames()
    {
        var patient = new Patient(
            Guid.NewGuid(),
            " P-001 ",
            " Jane ",
            " Doe ",
            new DateOnly(1990, 1, 1),
            Sex.Female,
            null,
            null,
            null,
            new Address(null, null, null, null),
            new NextOfKin(null, null, null));

        Assert.Equal("Jane", patient.FirstName);
        Assert.Equal("Doe", patient.LastName);
        Assert.Equal("P-001", patient.PatientNumber);
    }
}
