using ClinicAdmin.Application.Common.Validation;

namespace ClinicAdmin.Application.Authentication;

public sealed class LoginRequestValidator : IValidator<LoginRequest>
{
    public Task<IReadOnlyCollection<ValidationError>> ValidateAsync(LoginRequest instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(instance.Username))
        {
            errors.Add(new ValidationError(nameof(instance.Username), "Username is required."));
        }

        if (string.IsNullOrWhiteSpace(instance.Password))
        {
            errors.Add(new ValidationError(nameof(instance.Password), "Password is required."));
        }

        return Task.FromResult<IReadOnlyCollection<ValidationError>>(errors);
    }
}

