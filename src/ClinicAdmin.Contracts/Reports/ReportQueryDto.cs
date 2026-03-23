namespace ClinicAdmin.Contracts.Reports;

public sealed record ReportQueryDto(
    Guid FacilityId,
    DateOnly FromDate,
    DateOnly ToDate,
    int TopReasons = 5,
    int TopPatients = 10,
    int TopStaff = 10);
