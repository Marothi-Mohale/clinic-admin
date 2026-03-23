using ClinicAdmin.Contracts.Reports;
using ClinicAdmin.Desktop.Services;

namespace ClinicAdmin.Desktop.Tests.Services;

public sealed class ReportExportServiceTests
{
    [Fact]
    public async Task ExportOperationalReportCsvAsync_ShouldWriteCsvFile()
    {
        var exportRoot = Path.Combine(Path.GetTempPath(), "ClinicAdminTests", Guid.NewGuid().ToString("N"));
        var service = new ReportExportService(exportRoot);
        var report = new ClinicOperationalReportDto(
            new DateOnly(2026, 3, 20),
            new DateOnly(2026, 3, 21),
            2,
            4,
            new[] { new DailyRegistrationReportItemDto(new DateOnly(2026, 3, 20), 2) },
            new[] { new DailyVisitCountReportItemDto(new DateOnly(2026, 3, 20), 4) },
            new[] { new VisitReasonReportItemDto("Acute cough", 2) },
            new[] { new StaffActivityReportItemDto("RECEPTION", "Reception Clerk", "Receptionist", 1, 0, 2, 4, 0, 7) },
            new[] { new PatientVisitHistorySummaryReportItemDto(Guid.NewGuid(), "P-100", "Nomsa Dlamini", 2, new DateTimeOffset(2026, 3, 21, 8, 0, 0, TimeSpan.Zero), "Acute cough") });

        var path = await service.ExportOperationalReportCsvAsync(report);

        Assert.True(File.Exists(path));
        var contents = await File.ReadAllTextAsync(path);
        Assert.Contains("Clinic Administration Operational Report", contents);
        Assert.Contains("Daily Patient Registrations", contents);
        Assert.Contains("Acute cough", contents);
        Directory.Delete(exportRoot, recursive: true);
    }
}
