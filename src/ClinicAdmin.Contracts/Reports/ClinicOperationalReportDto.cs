namespace ClinicAdmin.Contracts.Reports;

public sealed record ClinicOperationalReportDto(
    DateOnly FromDate,
    DateOnly ToDate,
    int TotalRegistrations,
    int TotalVisits,
    IReadOnlyCollection<DailyRegistrationReportItemDto> DailyRegistrations,
    IReadOnlyCollection<DailyVisitCountReportItemDto> DailyVisits,
    IReadOnlyCollection<VisitReasonReportItemDto> CommonReasons,
    IReadOnlyCollection<StaffActivityReportItemDto> StaffActivity,
    IReadOnlyCollection<PatientVisitHistorySummaryReportItemDto> PatientVisitHistory);
