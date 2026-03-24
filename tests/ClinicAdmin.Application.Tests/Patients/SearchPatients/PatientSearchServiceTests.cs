using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Domain.Files;
using ClinicAdmin.Domain.Patients;
using ClinicAdmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Tests.Patients.SearchPatients;

public sealed class PatientSearchServiceTests
{
    private readonly Guid _facilityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task SearchAsync_WhenSearchingByPatientNumber_ShouldReturnMatchingPatient()
    {
        await using var dbContext = CreateDbContext();
        SeedPatients(dbContext);
        var service = new PatientSearchService(dbContext);

        var results = await service.SearchAsync(new SearchPatientsQuery(_facilityId, "P-900"));

        Assert.Single(results);
        Assert.Equal("P-900", results.Single().PatientNumber);
    }

    [Fact]
    public async Task SearchAsync_WhenSearchingByPartialSurname_ShouldReturnPartialMatches()
    {
        await using var dbContext = CreateDbContext();
        SeedPatients(dbContext);
        var service = new PatientSearchService(dbContext);

        var results = await service.SearchAsync(new SearchPatientsQuery(_facilityId, "Dlam"));

        Assert.NotEmpty(results);
        Assert.Contains(results, x => x.DisplayName.Contains("Dlamini", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetProfileAsync_ShouldReturnFileAndHistorySummary()
    {
        await using var dbContext = CreateDbContext();
        var patient = new Patient(
            _facilityId,
            "P-910",
            "Lerato",
            "Molefe",
            new DateOnly(1995, 8, 20),
            Sex.Female,
            "9508201234083",
            null,
            "0829999999",
            new Address("12 Main", null, "Mamelodi", "Pretoria"),
            new NextOfKin("Neo Molefe", "Brother", "0827777777"));
        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();
        dbContext.Files.Add(new FileRecord(patient.Id, _facilityId, "F-100"));
        dbContext.AuditEntries.Add(new ClinicAdmin.Domain.Auditing.AuditEntry(
            _facilityId,
            "RECEPTION",
            "PatientRegistered",
            nameof(Patient),
            patient.Id,
            "Patient registered",
            null,
            null,
            null,
            null,
            true,
            DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var service = new PatientSearchService(dbContext);

        var profile = await service.GetProfileAsync(_facilityId, patient.Id);

        Assert.NotNull(profile);
        Assert.Equal("F-100", profile!.FileNumber);
        Assert.NotEmpty(profile.History);
    }

    private ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }

    private void SeedPatients(ClinicAdminDbContext dbContext)
    {
        dbContext.Patients.AddRange(
            new Patient(_facilityId, "P-900", "Nomsa", "Dlamini", new DateOnly(1990, 1, 1), Sex.Female, "9001011234088", null, "0821234567", new Address(null, null, null, null), new NextOfKin(null, null, null)),
            new Patient(_facilityId, "P-901", "John", "Smith", new DateOnly(1987, 5, 5), Sex.Male, null, "A123456", "0832222222", new Address(null, null, null, null), new NextOfKin(null, null, null)));
        dbContext.SaveChanges();
    }
}
