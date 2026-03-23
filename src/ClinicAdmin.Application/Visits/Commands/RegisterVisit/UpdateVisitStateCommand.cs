using ClinicAdmin.Domain.Visits;

namespace ClinicAdmin.Application.Visits.Commands.RegisterVisit;

public sealed record UpdateVisitStateCommand(
    Guid FacilityId,
    Guid VisitId,
    QueueStatus QueueStatus,
    VisitState State,
    string? Department,
    string? AssignedStaffMember,
    string? Notes);

