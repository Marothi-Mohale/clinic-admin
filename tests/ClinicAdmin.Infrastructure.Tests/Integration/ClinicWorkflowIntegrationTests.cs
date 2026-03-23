using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using ClinicAdmin.Application.Reports.Queries;
using ClinicAdmin.Application.Visits.Commands.RegisterVisit;
using ClinicAdmin.Domain.Patients;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Domain.Visits;
using ClinicAdmin.Infrastructure.Auditing;
using ClinicAdmin.Infrastructure.Persistence;
using ClinicAdmin.Infrastructure.Security;
using ClinicAdmin.Infrastructure.Sync;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicAdmin.Infrastructure.Tests.Integration;

public sealed class ClinicWorkflowIntegrationTests
{
    private static readonly Guid FacilityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task LoginRegisterVisitAndReportFlow_ShouldPersistExpectedOperationalData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new ClinicAdminDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var facilityContext = new FakeFacilityContext();
        var sessionService = new UserSessionService();
        var clock = new FakeClock();
        var passwordHasher = new Pbkdf2PasswordHasher();
        var passwordHash = passwordHasher.HashPassword("Reception@123");
        dbContext.Users.Add(new AppUser(FacilityId, "reception", "Reception Clerk", passwordHash.Hash, passwordHash.Salt, UserRole.Receptionist));
        await dbContext.SaveChangesAsync();

        var auditService = new AuditService(
            dbContext,
            sessionService,
            clock,
            facilityContext,
            new FakeWorkstationContext(),
            NullLogger<AuditService>.Instance);

        var authenticationService = new AuthenticationService(
            dbContext,
            passwordHasher,
            sessionService,
            auditService,
            clock,
            facilityContext,
            new ValidatorExecutor<LoginRequest>(new[] { new LoginRequestValidator() }),
            NullLogger<AuthenticationService>.Instance);

        var loginResult = await authenticationService.LoginAsync("reception", "Reception@123");
        Assert.True(loginResult.Succeeded);

        var registrationService = new RegisterPatientCommandHandler(
            dbContext,
            auditService,
            new SyncJournalService(NullLogger<SyncJournalService>.Instance),
            new PatientRegistrationDuplicateWarningService(
                new PatientRegistrationDuplicateQueryService(dbContext),
                new PatientDuplicateDetectionService()),
            new ValidatorExecutor<RegisterPatientCommand>(new[] { new RegisterPatientCommandValidator() }));

        var registrationResult = await registrationService.RegisterAsync(new RegisterPatientCommand(
            FacilityId,
            "P-900",
            "Nomsa",
            "Dlamini",
            new DateOnly(1992, 4, 5),
            Sex.Female,
            "9204051234087",
            null,
            "0825555555",
            "12 Main",
            null,
            "Mamelodi",
            "Pretoria",
            "Thabo Dlamini",
            "Brother",
            "0829999999"));

        Assert.True(registrationResult.Succeeded);
        Assert.NotNull(registrationResult.PatientId);

        var visitWorkflowService = new VisitWorkflowService(
            dbContext,
            auditService,
            new SyncJournalService(NullLogger<SyncJournalService>.Instance),
            clock,
            new ValidatorExecutor<RegisterVisitCommand>(new[] { new RegisterVisitCommandValidator() }),
            new ValidatorExecutor<UpdateVisitStateCommand>(new[] { new UpdateVisitStateCommandValidator() }));

        var visitResult = await visitWorkflowService.RegisterArrivalAsync(new RegisterVisitCommand(
            FacilityId,
            registrationResult.PatientId!.Value,
            "Acute cough",
            QueueStatus.Waiting,
            VisitState.Registered,
            "Outpatients",
            "Nurse A",
            "Screen on arrival"));

        Assert.Equal("Acute cough", visitResult.ReasonForVisit);

        var reportingService = new ReportingService(dbContext);
        var report = await reportingService.GetOperationalReportAsync(new ClinicAdmin.Contracts.Reports.ReportQueryDto(
            FacilityId,
            new DateOnly(2026, 3, 23),
            new DateOnly(2026, 3, 23)));

        Assert.Equal(1, report.TotalRegistrations);
        Assert.Equal(1, report.TotalVisits);
        Assert.Contains(report.CommonReasons, x => x.ReasonForVisit == "Acute cough");
        Assert.Contains(report.StaffActivity, x => x.Username == "RECEPTION" && x.PatientRegistrations == 1 && x.VisitsRegistered == 1);
        Assert.Contains(dbContext.AuditEntries, x => x.Action == "PatientRegistered");
        Assert.Contains(dbContext.AuditEntries, x => x.Action == "VisitRegistered");
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => FacilityId;
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
