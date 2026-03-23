namespace ClinicAdmin.Contracts.Reports;

public sealed record DailyVisitCountReportItemDto(
    DateOnly Date,
    int VisitCount);
