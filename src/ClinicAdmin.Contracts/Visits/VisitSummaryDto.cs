namespace ClinicAdmin.Contracts.Visits;

public sealed record VisitSummaryDto(
    Guid Id,
    Guid PatientId,
    string PatientNumber,
    string PatientDisplayName,
    DateTimeOffset ArrivedAtUtc,
    string ReasonForVisit,
    string QueueStatus,
    string State,
    string? Department,
    string? AssignedStaffMember,
    string Notes);

