using ClinicAdmin.Contracts.Reports;

namespace ClinicAdmin.Application.Reports.Queries;

public interface IReportingService
{
    Task<ClinicOperationalReportDto> GetOperationalReportAsync(ReportQueryDto query, CancellationToken cancellationToken = default);
}
