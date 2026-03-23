namespace ClinicAdmin.Contracts.Patients;

public sealed record PatientHistoryItemDto(
    DateTimeOffset OccurredAtUtc,
    string Action,
    string Details,
    bool Succeeded);

