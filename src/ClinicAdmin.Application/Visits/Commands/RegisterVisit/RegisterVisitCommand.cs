using ClinicAdmin.Domain.Visits;

namespace ClinicAdmin.Application.Visits.Commands.RegisterVisit;

public sealed record RegisterVisitCommand(
    Guid FacilityId,
    Guid PatientId,
    string ReasonForVisit,
    QueueStatus QueueStatus,
    VisitState State,
    string? Department,
    string? AssignedStaffMember,
    string? Notes);

