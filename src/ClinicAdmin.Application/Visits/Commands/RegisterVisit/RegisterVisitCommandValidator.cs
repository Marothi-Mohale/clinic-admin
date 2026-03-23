using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Domain.Visits;

namespace ClinicAdmin.Application.Visits.Commands.RegisterVisit;

public sealed class RegisterVisitCommandValidator : IValidator<RegisterVisitCommand>
{
    public Task<IReadOnlyCollection<ValidationError>> ValidateAsync(RegisterVisitCommand instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (instance.FacilityId == Guid.Empty)
        {
            errors.Add(new ValidationError(nameof(instance.FacilityId), "Facility is required."));
        }

        if (instance.PatientId == Guid.Empty)
        {
            errors.Add(new ValidationError(nameof(instance.PatientId), "Patient is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.ReasonForVisit))
        {
            errors.Add(new ValidationError(nameof(instance.ReasonForVisit), "Reason for visit is required."));
        }

        if (instance.ReasonForVisit is { Length: > 200 })
        {
            errors.Add(new ValidationError(nameof(instance.ReasonForVisit), "Reason for visit is too long."));
        }

        if (instance.Notes is { Length: > 2000 })
        {
            errors.Add(new ValidationError(nameof(instance.Notes), "Visit notes are too long."));
        }

        if (instance.Department is { Length: > 100 })
        {
            errors.Add(new ValidationError(nameof(instance.Department), "Department is too long."));
        }

        if (instance.AssignedStaffMember is { Length: > 120 })
        {
            errors.Add(new ValidationError(nameof(instance.AssignedStaffMember), "Assigned staff member is too long."));
        }

        if (instance.State == VisitState.Completed && instance.QueueStatus != QueueStatus.Completed)
        {
            errors.Add(new ValidationError(nameof(instance.QueueStatus), "Completed visits must use the completed queue status."));
        }

        return Task.FromResult<IReadOnlyCollection<ValidationError>>(errors);
    }
}

