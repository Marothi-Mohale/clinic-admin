namespace ClinicAdmin.Contracts.Reports;

public sealed record PatientVisitHistorySummaryReportItemDto(
    Guid PatientId,
    string PatientNumber,
    string DisplayName,
    int VisitCount,
    DateTimeOffset LastVisitAtUtc,
    string LastReasonForVisit);
