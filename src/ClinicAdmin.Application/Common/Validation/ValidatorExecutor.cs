using ClinicAdmin.Application.Common.Exceptions;

namespace ClinicAdmin.Application.Common.Validation;

public sealed class ValidatorExecutor<T>
{
    private readonly IEnumerable<IValidator<T>> _validators;

    public ValidatorExecutor(IEnumerable<IValidator<T>> validators)
    {
        _validators = validators;
    }

    public async Task ValidateAndThrowAsync(T instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        foreach (var validator in _validators)
        {
            var validationErrors = await validator.ValidateAsync(instance, cancellationToken);
            if (validationErrors.Count > 0)
            {
                errors.AddRange(validationErrors);
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
