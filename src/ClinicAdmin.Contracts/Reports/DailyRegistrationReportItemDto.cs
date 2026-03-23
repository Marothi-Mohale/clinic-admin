namespace ClinicAdmin.Contracts.Reports;

public sealed record DailyRegistrationReportItemDto(
    DateOnly Date,
    int RegistrationCount);
