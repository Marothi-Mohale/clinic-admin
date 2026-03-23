namespace ClinicAdmin.Contracts.Reports;

public sealed record StaffActivityReportItemDto(
    string Username,
    string DisplayName,
    string Role,
    int SuccessfulLogins,
    int FailedLogins,
    int PatientRegistrations,
    int VisitsRegistered,
    int VisitsUpdated,
    int TotalActions);
