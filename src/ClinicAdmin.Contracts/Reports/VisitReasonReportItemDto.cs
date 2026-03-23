namespace ClinicAdmin.Contracts.Reports;

public sealed record VisitReasonReportItemDto(
    string ReasonForVisit,
    int VisitCount);
