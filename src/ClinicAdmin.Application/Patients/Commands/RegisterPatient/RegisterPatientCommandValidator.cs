using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Domain.Patients;

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

        if (string.IsNullOrWhiteSpace(instance.PatientNumber))
        {
            errors.Add(new ValidationError(nameof(instance.PatientNumber), "Patient number is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.FirstName))
        {
            errors.Add(new ValidationError(nameof(instance.FirstName), "First name is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.LastName))
        {
            errors.Add(new ValidationError(nameof(instance.LastName), "Last name is required."));
        }

        if (instance.DateOfBirth is null)
        {
            errors.Add(new ValidationError(nameof(instance.DateOfBirth), "Date of birth is required."));
        }
        else if (instance.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
        {
            errors.Add(new ValidationError(nameof(instance.DateOfBirth), "Date of birth cannot be in the future."));
        }

        if (instance.Sex == Sex.Unknown)
        {
            errors.Add(new ValidationError(nameof(instance.Sex), "Sex is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.NationalIdNumber) && string.IsNullOrWhiteSpace(instance.PassportNumber))
        {
            errors.Add(new ValidationError(nameof(instance.NationalIdNumber), "Capture a national ID number or passport number."));
        }

        if (instance.NationalIdNumber is { Length: > 13 })
        {
            errors.Add(new ValidationError(nameof(instance.NationalIdNumber), "National ID number is too long."));
        }
        else if (!string.IsNullOrWhiteSpace(instance.NationalIdNumber) && instance.NationalIdNumber.Any(character => !char.IsDigit(character)))
        {
            errors.Add(new ValidationError(nameof(instance.NationalIdNumber), "National ID number must contain digits only."));
        }

        if (instance.PassportNumber is { Length: > 20 })
        {
            errors.Add(new ValidationError(nameof(instance.PassportNumber), "Passport number is too long."));
        }

        if (instance.PhoneNumber is { Length: > 50 })
        {
            errors.Add(new ValidationError(nameof(instance.PhoneNumber), "Phone number is too long."));
        }
        else if (!string.IsNullOrWhiteSpace(instance.PhoneNumber))
        {
            var digits = new string(instance.PhoneNumber.Where(char.IsDigit).ToArray());
            if (digits.Length is < 10 or > 11)
            {
                errors.Add(new ValidationError(nameof(instance.PhoneNumber), "Phone number must contain 10 or 11 digits."));
            }
        }

        if (instance.NextOfKinPhoneNumber is { Length: > 50 })
        {
            errors.Add(new ValidationError(nameof(instance.NextOfKinPhoneNumber), "Next of kin phone number is too long."));
        }

        if (instance.PatientNumber is { Length: > 20 })
        {
            errors.Add(new ValidationError(nameof(instance.PatientNumber), "Patient number is too long."));
        }

        if (instance.AddressLine1 is { Length: > 150 })
        {
            errors.Add(new ValidationError(nameof(instance.AddressLine1), "Address line 1 is too long."));
        }

        if (instance.AddressLine2 is { Length: > 150 })
        {
            errors.Add(new ValidationError(nameof(instance.AddressLine2), "Address line 2 is too long."));
        }

        if (instance.Suburb is { Length: > 100 })
        {
            errors.Add(new ValidationError(nameof(instance.Suburb), "Suburb is too long."));
        }

        if (instance.City is { Length: > 100 })
        {
            errors.Add(new ValidationError(nameof(instance.City), "City is too long."));
        }

        if (instance.NextOfKinName is { Length: > 150 })
        {
            errors.Add(new ValidationError(nameof(instance.NextOfKinName), "Next of kin name is too long."));
        }

        if (instance.NextOfKinRelationship is { Length: > 100 })
        {
            errors.Add(new ValidationError(nameof(instance.NextOfKinRelationship), "Next of kin relationship is too long."));
        }

        return Task.FromResult<IReadOnlyCollection<ValidationError>>(errors);
    }
}
