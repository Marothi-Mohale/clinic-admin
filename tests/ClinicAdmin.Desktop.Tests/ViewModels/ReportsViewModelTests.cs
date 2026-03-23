using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Reports.Queries;
using ClinicAdmin.Contracts.Reports;
using ClinicAdmin.Desktop.Services;
using ClinicAdmin.Desktop.ViewModels;

namespace ClinicAdmin.Desktop.Tests.ViewModels;

public sealed class ReportsViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WhenReportLoads_ShouldPopulateTotalsAndCollections()
    {
        var viewModel = new ReportsViewModel(
            new FakeReportingService(),
            new FakeReportExportService(),
            new FakeFacilityContext());

        await viewModel.InitializeAsync();

        Assert.Equal(3, viewModel.TotalRegistrations);
        Assert.Equal(5, viewModel.TotalVisits);
        Assert.Single(viewModel.CommonReasons);
        Assert.True(viewModel.HasReportData);
    }

    [Fact]
    public async Task ExportCsvAsync_WhenDataExists_ShouldUpdateStatusMessage()
    {
        var viewModel = new ReportsViewModel(
            new FakeReportingService(),
            new FakeReportExportService(),
            new FakeFacilityContext());

        await viewModel.InitializeAsync();
        viewModel.ExportCsvCommand.Execute(null);
        await Task.Delay(50);

        Assert.Contains("exported", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeReportingService : IReportingService
    {
        public Task<ClinicOperationalReportDto> GetOperationalReportAsync(ReportQueryDto query, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ClinicOperationalReportDto(
                query.FromDate,
                query.ToDate,
                3,
                5,
                new[] { new DailyRegistrationReportItemDto(query.FromDate, 3) },
                new[] { new DailyVisitCountReportItemDto(query.FromDate, 5) },
                new[] { new VisitReasonReportItemDto("Acute cough", 2) },
                new[] { new StaffActivityReportItemDto("RECEPTION", "Reception Clerk", "Receptionist", 1, 0, 3, 5, 1, 10) },
                new[] { new PatientVisitHistorySummaryReportItemDto(Guid.NewGuid(), "P-100", "Nomsa Dlamini", 2, new DateTimeOffset(2026, 3, 23, 8, 0, 0, TimeSpan.Zero), "Acute cough") }));
    }

    private sealed class FakeReportExportService : IReportExportService
    {
        public Task<string> ExportOperationalReportCsvAsync(ClinicOperationalReportDto report, CancellationToken cancellationToken = default) =>
            Task.FromResult(@"C:\Exports\clinic-report.csv");
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }
}
