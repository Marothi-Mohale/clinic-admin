using System.Text;
using ClinicAdmin.Contracts.Reports;

namespace ClinicAdmin.Desktop.Services;

public sealed class ReportExportService : IReportExportService
{
    private readonly string _exportRootDirectory;

    public ReportExportService(string? exportRootDirectory = null)
    {
        _exportRootDirectory = string.IsNullOrWhiteSpace(exportRootDirectory)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ClinicAdmin",
                "Exports")
            : exportRootDirectory;
    }

    public async Task<string> ExportOperationalReportCsvAsync(ClinicOperationalReportDto report, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_exportRootDirectory);

        var fileName = $"clinic-report-{report.FromDate:yyyyMMdd}-{report.ToDate:yyyyMMdd}.csv";
        var fullPath = Path.Combine(_exportRootDirectory, fileName);

        var builder = new StringBuilder();
        builder.AppendLine("Clinic Administration Operational Report");
        builder.AppendLine($"Date Range,{report.FromDate:yyyy-MM-dd} to {report.ToDate:yyyy-MM-dd}");
        builder.AppendLine($"Total Registrations,{report.TotalRegistrations}");
        builder.AppendLine($"Total Visits,{report.TotalVisits}");
        builder.AppendLine();

        AppendSection(builder, "Daily Patient Registrations", "Date,Registrations", report.DailyRegistrations.Select(x =>
            $"{x.Date:yyyy-MM-dd},{x.RegistrationCount}"));

        AppendSection(builder, "Total Visits Per Day", "Date,Visits", report.DailyVisits.Select(x =>
            $"{x.Date:yyyy-MM-dd},{x.VisitCount}"));

        AppendSection(builder, "Common Reasons For Visit", "Reason,Visits", report.CommonReasons.Select(x =>
            $"{Escape(x.ReasonForVisit)},{x.VisitCount}"));

        AppendSection(builder, "Staff Activity Summary", "Username,Display Name,Role,Successful Logins,Failed Logins,Patient Registrations,Visits Registered,Visits Updated,Total Actions", report.StaffActivity.Select(x =>
            $"{Escape(x.Username)},{Escape(x.DisplayName)},{Escape(x.Role)},{x.SuccessfulLogins},{x.FailedLogins},{x.PatientRegistrations},{x.VisitsRegistered},{x.VisitsUpdated},{x.TotalActions}"));

        AppendSection(builder, "Patient Visit History Summary", "Patient Number,Patient Name,Visit Count,Last Visit UTC,Last Reason", report.PatientVisitHistory.Select(x =>
            $"{Escape(x.PatientNumber)},{Escape(x.DisplayName)},{x.VisitCount},{x.LastVisitAtUtc:yyyy-MM-dd HH:mm:ss},{Escape(x.LastReasonForVisit)}"));

        await File.WriteAllTextAsync(fullPath, builder.ToString(), Encoding.UTF8, cancellationToken);
        return fullPath;
    }

    private static void AppendSection(StringBuilder builder, string title, string header, IEnumerable<string> rows)
    {
        builder.AppendLine(title);
        builder.AppendLine(header);

        foreach (var row in rows)
        {
            builder.AppendLine(row);
        }

        builder.AppendLine();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var escaped = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }
}
