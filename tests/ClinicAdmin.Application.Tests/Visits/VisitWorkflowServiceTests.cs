using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Visits.Commands.RegisterVisit;
using ClinicAdmin.Domain.Patients;
using ClinicAdmin.Domain.Visits;
using ClinicAdmin.Infrastructure.Auditing;
using ClinicAdmin.Infrastructure.Persistence;
using ClinicAdmin.Infrastructure.Security;
using ClinicAdmin.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicAdmin.Application.Tests.Visits;

public sealed class VisitWorkflowServiceTests
{
    private readonly Guid _facilityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task RegisterArrivalAsync_WhenValid_ShouldPersistVisit()
    {
        await using var dbContext = CreateDbContext();
        var patient = SeedPatient(dbContext, "P-800");
        var service = CreateService(dbContext);

        var visit = await service.RegisterArrivalAsync(new RegisterVisitCommand(
            _facilityId,
            patient.Id,
            "Acute cough",
            QueueStatus.Waiting,
            VisitState.Registered,
            "Outpatients",
            "Nurse Khumalo",
            "Needs screening"));

        Assert.Equal("Acute cough", visit.ReasonForVisit);
        Assert.Single(dbContext.Visits);
        Assert.Contains(dbContext.AuditEntries, x => x.Action == "VisitRegistered");
    }

    [Fact]
    public async Task RegisterArrivalAsync_WhenActiveVisitExists_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var patient = SeedPatient(dbContext, "P-801");
        dbContext.Visits.Add(new PatientVisit(patient.Id, _facilityId, "Review", QueueStatus.Waiting, VisitState.Registered, null, null, null, DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterArrivalAsync(new RegisterVisitCommand(
            _facilityId,
            patient.Id,
            "Acute cough",
            QueueStatus.Waiting,
            VisitState.Registered,
            null,
            null,
            null)));
    }

    [Fact]
    public async Task UpdateVisitAsync_ShouldUpdateStateAndQueueStatus()
    {
        await using var dbContext = CreateDbContext();
        var patient = SeedPatient(dbContext, "P-802");
        var visit = new PatientVisit(patient.Id, _facilityId, "Check up", QueueStatus.Waiting, VisitState.Registered, null, null, null, DateTimeOffset.UtcNow);
        dbContext.Visits.Add(visit);
        await dbContext.SaveChangesAsync();
        var service = CreateService(dbContext);

        var updated = await service.UpdateVisitAsync(new UpdateVisitStateCommand(
            _facilityId,
            visit.Id,
            QueueStatus.Consultation,
            VisitState.InProgress,
            "Consulting room 2",
            "Dr Moyo",
            "Vitals done"));

        Assert.Equal("InProgress", updated.State);
        Assert.Equal("Consultation", updated.QueueStatus);
        Assert.Contains(dbContext.AuditEntries, x => x.Action == "VisitUpdated" && x.BeforeSummary != null && x.AfterSummary != null);
    }

    private ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }

    private Patient SeedPatient(ClinicAdminDbContext dbContext, string patientNumber)
    {
        var patient = new Patient(
            _facilityId,
            patientNumber,
            "Nomsa",
            "Dlamini",
            new DateOnly(1992, 4, 5),
            Sex.Female,
            "9204051234087",
            null,
            "0825555555",
            new Address("12 Main", null, "Mamelodi", "Pretoria"),
            new NextOfKin("Thabo Dlamini", "Brother", "0829999999"));
        dbContext.Patients.Add(patient);
        dbContext.SaveChanges();
        return patient;
    }

    private VisitWorkflowService CreateService(ClinicAdminDbContext dbContext)
    {
        var facilityContext = new FakeFacilityContext(_facilityId);
        var sessionService = new UserSessionService();
        var auditService = new AuditService(dbContext, sessionService, new FakeClock(), facilityContext, new FakeWorkstationContext(), NullLogger<AuditService>.Instance);

        return new VisitWorkflowService(
            dbContext,
            auditService,
            new SyncJournalService(NullLogger<SyncJournalService>.Instance),
            new FakeClock(),
            new ValidatorExecutor<RegisterVisitCommand>(new[] { new RegisterVisitCommandValidator() }),
            new ValidatorExecutor<UpdateVisitStateCommand>(new[] { new UpdateVisitStateCommandValidator() }));
    }

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
        public DateTimeOffset UtcNow => new(2026, 3, 23, 9, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeWorkstationContext : IWorkstationContext
    {
        public string WorkstationName => "TEST-WS";
    }
}
