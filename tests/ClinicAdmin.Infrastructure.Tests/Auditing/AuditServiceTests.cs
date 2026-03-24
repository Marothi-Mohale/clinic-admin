using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Auditing;
using ClinicAdmin.Infrastructure.Auditing;
using ClinicAdmin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicAdmin.Infrastructure.Tests.Auditing;

public sealed class AuditServiceTests
{
    [Fact]
    public async Task WriteChangeAsync_ShouldPersistStructuredAuditFields()
    {
        await using var dbContext = CreateDbContext();
        var sessionService = new FakeCurrentUserService("RECEPTION");
        var service = new AuditService(
            dbContext,
            sessionService,
            new FakeClock(),
            new FakeFacilityContext(),
            new FakeWorkstationContext(),
            NullLogger<AuditService>.Instance);

        await service.WriteChangeAsync(
            "PatientUpdated",
            "Patient",
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            "Updated patient contact details",
            beforeSummary: "Phone: 0821111111",
            afterSummary: "Phone: 0822222222",
            metadata: "{\"category\":\"patient-update\"}");

        var entry = Assert.Single(dbContext.AuditEntries);
        Assert.Equal("RECEPTION", entry.ActorUsername);
        Assert.Equal("PatientUpdated", entry.Action);
        Assert.Equal("Patient", entry.EntityName);
        Assert.Equal("Phone: 0821111111", entry.BeforeSummary);
        Assert.Equal("Phone: 0822222222", entry.AfterSummary);
        Assert.Equal("{\"category\":\"patient-update\"}", entry.Metadata);
        Assert.Equal("TEST-WS", entry.Workstation);
    }

    private static ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        public FakeCurrentUserService(string username)
        {
            Username = username;
        }

        public string Username { get; }
        public IReadOnlyCollection<string> Roles => ["RECEPTIONIST"];
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 3, 23, 10, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeWorkstationContext : IWorkstationContext
    {
        public string WorkstationName => "TEST-WS";
    }
}
