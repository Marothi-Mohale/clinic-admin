using ClinicAdmin.Application.Auditing;
using ClinicAdmin.Contracts.Auditing;
using ClinicAdmin.Domain.Auditing;
using ClinicAdmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Tests.Auditing;

public sealed class AuditLogQueryServiceTests
{
    private static readonly Guid FacilityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task QueryAsync_ShouldFilterByFacilityAndAction()
    {
        await using var dbContext = CreateDbContext();
        dbContext.AuditEntries.AddRange(
            new AuditEntry(FacilityId, "ADMIN", "PatientRegistered", "Patient", Guid.NewGuid(), "Created patient", null, "P-100", null, "WS-1", true, new DateTimeOffset(2026, 3, 22, 8, 0, 0, TimeSpan.Zero)),
            new AuditEntry(FacilityId, "ADMIN", "VisitRegistered", "PatientVisit", Guid.NewGuid(), "Created visit", null, "Review", null, "WS-1", true, new DateTimeOffset(2026, 3, 23, 8, 0, 0, TimeSpan.Zero)),
            new AuditEntry(Guid.Parse("22222222-2222-2222-2222-222222222222"), "ADMIN", "PatientRegistered", "Patient", Guid.NewGuid(), "Other facility", null, "P-200", null, "WS-2", true, new DateTimeOffset(2026, 3, 23, 9, 0, 0, TimeSpan.Zero)));
        await dbContext.SaveChangesAsync();

        var service = new AuditLogQueryService(dbContext);

        var results = await service.QueryAsync(new AuditLogQueryDto(FacilityId, null, "patientregistered", null, null, null, null, 50));

        var entry = Assert.Single(results);
        Assert.Equal("PatientRegistered", entry.Action);
        Assert.Equal("Patient", entry.EntityName);
    }

    [Fact]
    public async Task QueryAsync_ShouldRespectSearchTermAndNewestFirstOrdering()
    {
        await using var dbContext = CreateDbContext();
        dbContext.AuditEntries.AddRange(
            new AuditEntry(FacilityId, "RECEPTION", "AuthenticationAttempt", "AppUser", null, "Login failed", null, null, "{\"category\":\"authentication\"}", "WS-1", false, new DateTimeOffset(2026, 3, 21, 8, 0, 0, TimeSpan.Zero)),
            new AuditEntry(FacilityId, "RECEPTION", "PatientRegistered", "Patient", Guid.NewGuid(), "Created Nomsa Dlamini", null, "P-300", "{\"category\":\"registration\"}", "WS-1", true, new DateTimeOffset(2026, 3, 23, 8, 0, 0, TimeSpan.Zero)));
        await dbContext.SaveChangesAsync();

        var service = new AuditLogQueryService(dbContext);

        var results = await service.QueryAsync(new AuditLogQueryDto(FacilityId, "nomsa", null, null, null, null, null, 50));

        var entry = Assert.Single(results);
        Assert.Equal("PatientRegistered", entry.Action);
        Assert.Equal("Created Nomsa Dlamini", entry.Details);
    }

    private static ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }
}
