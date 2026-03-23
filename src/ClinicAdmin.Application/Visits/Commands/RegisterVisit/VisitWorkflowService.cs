using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Contracts.Visits;
using ClinicAdmin.Domain.Visits;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Visits.Commands.RegisterVisit;

public sealed class VisitWorkflowService : IVisitWorkflowService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ISyncJournal _syncJournal;
    private readonly IClock _clock;
    private readonly ValidatorExecutor<RegisterVisitCommand> _registerValidator;
    private readonly ValidatorExecutor<UpdateVisitStateCommand> _updateValidator;

    public VisitWorkflowService(
        IApplicationDbContext dbContext,
        IAuditService auditService,
        ISyncJournal syncJournal,
        IClock clock,
        ValidatorExecutor<RegisterVisitCommand> registerValidator,
        ValidatorExecutor<UpdateVisitStateCommand> updateValidator)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _syncJournal = syncJournal;
        _clock = clock;
        _registerValidator = registerValidator;
        _updateValidator = updateValidator;
    }

    public async Task<VisitSummaryDto> RegisterArrivalAsync(RegisterVisitCommand command, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(command, cancellationToken);

        var patient = await _dbContext.Patients
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.FacilityId == command.FacilityId && x.Id == command.PatientId, cancellationToken);

        if (patient is null)
        {
            throw new InvalidOperationException("The selected patient could not be found.");
        }

        var activeVisitExists = await _dbContext.Visits.AnyAsync(
            x => x.FacilityId == command.FacilityId &&
                 x.PatientId == command.PatientId &&
                 x.State != VisitState.Completed &&
                 x.State != VisitState.Cancelled,
            cancellationToken);

        if (activeVisitExists)
        {
            throw new InvalidOperationException("This patient already has an active visit. Update the existing visit instead of registering a new arrival.");
        }

        var arrivedAtUtc = _clock.UtcNow;
        var visit = new PatientVisit(
            command.PatientId,
            command.FacilityId,
            command.ReasonForVisit,
            command.QueueStatus,
            command.State,
            command.Department,
            command.AssignedStaffMember,
            command.Notes,
            arrivedAtUtc);

        _dbContext.Visits.Add(visit);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.WriteChangeAsync(
            "VisitRegistered",
            nameof(PatientVisit),
            visit.Id,
            $"{patient.PatientNumber} {command.ReasonForVisit}",
            afterSummary: BuildVisitSummary(visit),
            metadata: $"{{\"patientId\":\"{visit.PatientId}\",\"queueStatus\":\"{visit.QueueStatus}\",\"state\":\"{visit.State}\"}}",
            cancellationToken: cancellationToken);
        await _syncJournal.EnqueueAsync(
            "VisitRegistered",
            nameof(PatientVisit),
            visit.Id,
            command.FacilityId,
            $"{{\"visitId\":\"{visit.Id}\",\"patientId\":\"{visit.PatientId}\",\"facilityId\":\"{visit.FacilityId}\"}}",
            cancellationToken);

        return ToSummaryDto(visit, patient.PatientNumber, $"{patient.FirstName} {patient.LastName}");
    }

    public async Task<VisitSummaryDto> UpdateVisitAsync(UpdateVisitStateCommand command, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var visit = await _dbContext.Visits
            .SingleOrDefaultAsync(x => x.FacilityId == command.FacilityId && x.Id == command.VisitId, cancellationToken);

        if (visit is null)
        {
            throw new InvalidOperationException("The visit could not be found.");
        }

        var beforeSummary = BuildVisitSummary(visit);

        visit.UpdateWorkflow(
            command.QueueStatus,
            command.State,
            command.Department,
            command.AssignedStaffMember,
            command.Notes,
            _clock.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var patient = await _dbContext.Patients
            .AsNoTracking()
            .SingleAsync(x => x.Id == visit.PatientId, cancellationToken);

        await _auditService.WriteChangeAsync(
            "VisitUpdated",
            nameof(PatientVisit),
            visit.Id,
            $"{patient.PatientNumber} {command.State} {command.QueueStatus}",
            beforeSummary: beforeSummary,
            afterSummary: BuildVisitSummary(visit),
            metadata: $"{{\"patientId\":\"{visit.PatientId}\",\"queueStatus\":\"{visit.QueueStatus}\",\"state\":\"{visit.State}\"}}",
            cancellationToken: cancellationToken);

        return ToSummaryDto(visit, patient.PatientNumber, $"{patient.FirstName} {patient.LastName}");
    }

    public async Task<IReadOnlyCollection<VisitHistoryItemDto>> GetVisitHistoryAsync(Guid facilityId, Guid patientId, int take = 20, CancellationToken cancellationToken = default)
    {
        var cappedTake = Math.Clamp(take, 1, 50);

        return await _dbContext.Visits
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.PatientId == patientId)
            .OrderByDescending(x => x.ArrivedAtUtc)
            .Take(cappedTake)
            .Select(x => new VisitHistoryItemDto(
                x.Id,
                x.ArrivedAtUtc,
                x.ReasonForVisit,
                x.QueueStatus.ToString(),
                x.State.ToString(),
                x.Department,
                x.AssignedStaffMember,
                x.Notes))
            .ToListAsync(cancellationToken);
    }

    private static VisitSummaryDto ToSummaryDto(PatientVisit visit, string patientNumber, string patientDisplayName) =>
        new(
            visit.Id,
            visit.PatientId,
            patientNumber,
            patientDisplayName,
            visit.ArrivedAtUtc,
            visit.ReasonForVisit,
            visit.QueueStatus.ToString(),
            visit.State.ToString(),
            visit.Department,
            visit.AssignedStaffMember,
            visit.Notes);

    private static string BuildVisitSummary(PatientVisit visit) =>
        $"{visit.ReasonForVisit} | Queue: {visit.QueueStatus} | State: {visit.State} | Department: {visit.Department ?? "Unassigned"} | Staff: {visit.AssignedStaffMember ?? "Unassigned"}";
}
