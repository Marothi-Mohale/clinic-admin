using ClinicAdmin.Application.Common.Validation;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed class RegisterPatientCommandValidator : IValidator<RegisterPatientCommand>
{
    public Task<IReadOnlyCollection<ValidationError>> ValidateAsync(
        RegisterPatientCommand instance,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (instance.FacilityId == Guid.Empty)
        {
            errors.Add(new ValidationError(nameof(instance.FacilityId), "Facility is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.FirstName))
        {
            errors.Add(new ValidationError(nameof(instance.FirstName), "First name is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.LastName))
        {
            errors.Add(new ValidationError(nameof(instance.LastName), "Last name is required."));
        }

        if (instance.NationalIdNumber is { Length: > 13 })
        {
            errors.Add(new ValidationError(nameof(instance.NationalIdNumber), "National ID number is too long."));
        }

        if (instance.PassportNumber is { Length: > 20 })
        {
            errors.Add(new ValidationError(nameof(instance.PassportNumber), "Passport number is too long."));
        }

        if (instance.PhoneNumber is { Length: > 50 })
        {
            errors.Add(new ValidationError(nameof(instance.PhoneNumber), "Phone number is too long."));
        }

        return Task.FromResult<IReadOnlyCollection<ValidationError>>(errors);
    }
}
