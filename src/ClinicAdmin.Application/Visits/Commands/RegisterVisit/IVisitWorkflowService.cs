using ClinicAdmin.Contracts.Visits;

namespace ClinicAdmin.Application.Visits.Commands.RegisterVisit;

public interface IVisitWorkflowService
{
    Task<VisitSummaryDto> RegisterArrivalAsync(RegisterVisitCommand command, CancellationToken cancellationToken = default);
    Task<VisitSummaryDto> UpdateVisitAsync(UpdateVisitStateCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<VisitHistoryItemDto>> GetVisitHistoryAsync(Guid facilityId, Guid patientId, int take = 20, CancellationToken cancellationToken = default);
}

