using ClinicAdmin.Domain.Patients;

namespace ClinicAdmin.Domain.Tests.Patients;

public sealed class PatientTests
{
    [Fact]
    public void Constructor_ShouldTrimNames()
    {
        var patient = new Patient(Guid.NewGuid(), " Jane ", " Doe ", new DateOnly(1990, 1, 1), null, null, null);

        Assert.Equal("Jane", patient.FirstName);
        Assert.Equal("Doe", patient.LastName);
    }
}
