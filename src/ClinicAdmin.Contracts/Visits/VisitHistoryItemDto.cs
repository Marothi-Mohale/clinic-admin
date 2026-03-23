namespace ClinicAdmin.Contracts.Visits;

public sealed record VisitHistoryItemDto(
    Guid Id,
    DateTimeOffset ArrivedAtUtc,
    string ReasonForVisit,
    string QueueStatus,
    string State,
    string? Department,
    string? AssignedStaffMember,
    string Notes);

