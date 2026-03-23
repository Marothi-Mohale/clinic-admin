using ClinicAdmin.Application.Common.Validation;

namespace ClinicAdmin.Application.Common.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyCollection<ValidationError> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyCollection<ValidationError> Errors { get; }
}
