using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Domain.Visits;

namespace ClinicAdmin.Application.Visits.Commands.RegisterVisit;

public sealed class UpdateVisitStateCommandValidator : IValidator<UpdateVisitStateCommand>
{
    public Task<IReadOnlyCollection<ValidationError>> ValidateAsync(UpdateVisitStateCommand instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (instance.FacilityId == Guid.Empty)
        {
            errors.Add(new ValidationError(nameof(instance.FacilityId), "Facility is required."));
        }

        if (instance.VisitId == Guid.Empty)
        {
            errors.Add(new ValidationError(nameof(instance.VisitId), "Visit is required."));
        }

        if (instance.Notes is { Length: > 2000 })
        {
            errors.Add(new ValidationError(nameof(instance.Notes), "Visit notes are too long."));
        }

        if (instance.State == VisitState.Completed && instance.QueueStatus != QueueStatus.Completed)
        {
            errors.Add(new ValidationError(nameof(instance.QueueStatus), "Completed visits must use the completed queue status."));
        }

        return Task.FromResult<IReadOnlyCollection<ValidationError>>(errors);
    }
}

