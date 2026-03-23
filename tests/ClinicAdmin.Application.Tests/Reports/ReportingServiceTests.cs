using ClinicAdmin.Application.Reports.Queries;
using ClinicAdmin.Contracts.Reports;
using ClinicAdmin.Domain.Auditing;
using ClinicAdmin.Domain.Patients;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Domain.Visits;
using ClinicAdmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Tests.Reports;

public sealed class ReportingServiceTests
{
    private static readonly Guid FacilityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetOperationalReportAsync_ShouldReturnGroupedMvpMetrics()
    {
        await using var dbContext = CreateDbContext();
        var patient = new Patient(
            FacilityId,
            "P-100",
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
        dbContext.Users.Add(new AppUser(FacilityId, "RECEPTION", "Reception Clerk", "hash", "salt", UserRole.Receptionist));
        dbContext.Visits.AddRange(
            new PatientVisit(patient.Id, FacilityId, "Acute cough", QueueStatus.Waiting, VisitState.Registered, "OPD", "Nurse A", null, new DateTimeOffset(2026, 3, 20, 8, 0, 0, TimeSpan.Zero)),
            new PatientVisit(patient.Id, FacilityId, "Medication collection", QueueStatus.Completed, VisitState.Completed, "Pharmacy", "Nurse B", null, new DateTimeOffset(2026, 3, 21, 9, 0, 0, TimeSpan.Zero)));
        dbContext.AuditEntries.AddRange(
            new AuditEntry(FacilityId, "RECEPTION", "PatientRegistered", "Patient", patient.Id, "Created patient", null, "P-100", null, "WS-1", true, new DateTimeOffset(2026, 3, 20, 7, 30, 0, TimeSpan.Zero)),
            new AuditEntry(FacilityId, "RECEPTION", "VisitRegistered", "PatientVisit", Guid.NewGuid(), "Created visit", null, "Acute cough", null, "WS-1", true, new DateTimeOffset(2026, 3, 20, 8, 5, 0, TimeSpan.Zero)),
            new AuditEntry(FacilityId, "RECEPTION", "VisitUpdated", "PatientVisit", Guid.NewGuid(), "Updated visit", "Waiting", "Completed", null, "WS-1", true, new DateTimeOffset(2026, 3, 21, 10, 0, 0, TimeSpan.Zero)),
            new AuditEntry(FacilityId, "RECEPTION", "AuthenticationAttempt", "AppUser", null, "Login succeeded", null, null, null, "WS-1", true, new DateTimeOffset(2026, 3, 21, 6, 50, 0, TimeSpan.Zero)));
        await dbContext.SaveChangesAsync();

        var service = new ReportingService(dbContext);

        var report = await service.GetOperationalReportAsync(new ReportQueryDto(
            FacilityId,
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 21)));

        Assert.Equal(1, report.TotalRegistrations);
        Assert.Equal(2, report.TotalVisits);
        Assert.Contains(report.CommonReasons, x => x.ReasonForVisit == "Acute cough" && x.VisitCount == 1);
        Assert.Contains(report.StaffActivity, x => x.Username == "RECEPTION" && x.PatientRegistrations == 1 && x.SuccessfulLogins == 1);
        Assert.Contains(report.PatientVisitHistory, x => x.PatientNumber == "P-100" && x.VisitCount == 2);
    }

    private static ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }
}
