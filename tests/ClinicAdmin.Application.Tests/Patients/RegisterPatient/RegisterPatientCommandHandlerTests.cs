using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using ClinicAdmin.Domain.Patients;
using ClinicAdmin.Infrastructure.Auditing;
using ClinicAdmin.Infrastructure.Persistence;
using ClinicAdmin.Infrastructure.Security;
using ClinicAdmin.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicAdmin.Application.Tests.Patients.RegisterPatient;

public sealed class RegisterPatientCommandHandlerTests
{
    private readonly Guid _facilityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ShouldPersistPatient()
    {
        await using var dbContext = CreateDbContext();
        var handler = CreateHandler(dbContext);

        var result = await handler.HandleAsync(CreateCommand("P-200", "Alice", "Mabaso"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.PatientId);
        Assert.Single(dbContext.Patients);
        Assert.Equal("P-200", dbContext.Patients.Single().PatientNumber);
        Assert.True(dbContext.AuditEntries.Any());
    }

    [Fact]
    public async Task HandleAsync_WhenPatientNumberExists_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Patients.Add(new Patient(
            _facilityId,
            "P-300",
            "Existing",
            "Patient",
            new DateOnly(1990, 1, 1),
            Sex.Female,
            "9001011234088",
            null,
            "0821230000",
            new Address("1 Main", null, null, "Pretoria"),
            new NextOfKin("Relative", "Mother", "0821239999")));
        await dbContext.SaveChangesAsync();

        var handler = CreateHandler(dbContext);
        var result = await handler.HandleAsync(CreateCommand("P-300", "New", "Patient"));

        Assert.False(result.Succeeded);
        Assert.Contains("already in use", result.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateWarningRequiresConfirmation_ShouldNotPersistUntilAcknowledged()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Patients.Add(new Patient(
            _facilityId,
            "P-401",
            "Ayanda",
            "Zulu",
            null,
            Sex.Female,
            null,
            null,
            null,
            new Address(null, null, null, null),
            new NextOfKin(null, null, null)));
        await dbContext.SaveChangesAsync();

        var handler = CreateHandler(dbContext);
        var firstAttempt = await handler.HandleAsync(CreateCommand("P-402", "Ayanda", "Zulu"));

        Assert.False(firstAttempt.Succeeded);
        Assert.True(firstAttempt.RequiresConfirmation);
        Assert.Empty(dbContext.Patients.Where(x => x.PatientNumber == "P-402"));

        var secondAttempt = await handler.HandleAsync(CreateCommand("P-402", "Ayanda", "Zulu", duplicateWarningAcknowledged: true));

        Assert.True(secondAttempt.Succeeded);
        Assert.Single(dbContext.Patients.Where(x => x.PatientNumber == "P-402"));
    }

    private ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }

    private RegisterPatientCommandHandler CreateHandler(ClinicAdminDbContext dbContext)
    {
        var facilityContext = new FakeFacilityContext(_facilityId);
        var sessionService = new UserSessionService();
        var auditService = new AuditService(dbContext, sessionService, new FakeClock(), facilityContext, new FakeWorkstationContext(), NullLogger<AuditService>.Instance);
        var duplicateQueryService = new PatientRegistrationDuplicateQueryService(dbContext);
        var duplicateWarningService = new PatientRegistrationDuplicateWarningService(duplicateQueryService, new PatientDuplicateDetectionService());

        return new RegisterPatientCommandHandler(
            dbContext,
            auditService,
            new SyncJournalService(NullLogger<SyncJournalService>.Instance),
            duplicateWarningService,
            new ValidatorExecutor<RegisterPatientCommand>(new[] { new RegisterPatientCommandValidator() }));
    }

    private RegisterPatientCommand CreateCommand(string patientNumber, string firstName, string lastName, bool duplicateWarningAcknowledged = false) =>
        new(
            _facilityId,
            patientNumber,
            firstName,
            lastName,
            new DateOnly(1994, 6, 10),
            Sex.Female,
            "9406101234088",
            null,
            "0821234567",
            "12 Main Street",
            null,
            "Mamelodi",
            "Pretoria",
            "Sarah Mabaso",
            "Sister",
            "0827654321",
            duplicateWarningAcknowledged);

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public FakeFacilityContext(Guid facilityId)
        {
            CurrentFacilityId = facilityId;
        }

        public Guid CurrentFacilityId { get; }
        public string FacilityCode => "MAIN";
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 3, 23, 8, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeWorkstationContext : IWorkstationContext
    {
        public string WorkstationName => "TEST-WS";
    }
}
