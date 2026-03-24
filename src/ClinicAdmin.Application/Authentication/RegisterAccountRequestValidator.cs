using ClinicAdmin.Application.Common.Validation;

namespace ClinicAdmin.Application.Authentication;

public sealed class RegisterAccountRequestValidator : IValidator<RegisterAccountRequest>
{
    public Task<IReadOnlyCollection<ValidationError>> ValidateAsync(RegisterAccountRequest instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(instance.FileNumber))
        {
            errors.Add(new ValidationError(nameof(instance.FileNumber), "File number is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.Username))
        {
            errors.Add(new ValidationError(nameof(instance.Username), "Username is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.Password))
        {
            errors.Add(new ValidationError(nameof(instance.Password), "Password is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.IdNumber))
        {
            errors.Add(new ValidationError(nameof(instance.IdNumber), "ID number is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.Email))
        {
            errors.Add(new ValidationError(nameof(instance.Email), "Email is required."));
        }
        else if (!LooksLikeEmail(instance.Email))
        {
            errors.Add(new ValidationError(nameof(instance.Email), "Email format is invalid."));
        }

        if (!string.Equals(instance.IdNumber?.Trim(), instance.ConfirmedIdNumber?.Trim(), StringComparison.Ordinal))
        {
            errors.Add(new ValidationError(nameof(instance.ConfirmedIdNumber), "ID number confirmation does not match."));
        }

        if (!string.Equals(instance.Email?.Trim(), instance.ConfirmedEmail?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new ValidationError(nameof(instance.ConfirmedEmail), "Email confirmation does not match."));
        }

        return Task.FromResult<IReadOnlyCollection<ValidationError>>(errors);
    }

    private static bool LooksLikeEmail(string value)
    {
        var email = value.Trim();
        var atIndex = email.IndexOf('@');
        var dotIndex = email.LastIndexOf('.');
        return atIndex > 0 && dotIndex > atIndex + 1 && dotIndex < email.Length - 1;
    }
}
