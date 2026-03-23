using ClinicAdmin.Contracts.Reports;

namespace ClinicAdmin.Desktop.Services;

public interface IReportExportService
{
    Task<string> ExportOperationalReportCsvAsync(ClinicOperationalReportDto report, CancellationToken cancellationToken = default);
}
