using ClinicAdmin.Application.Common.Validation;

namespace ClinicAdmin.Application.Common.Results;

public sealed class OperationResult
{
    private OperationResult(bool succeeded, IReadOnlyCollection<ValidationError> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyCollection<ValidationError> Errors { get; }

    public static OperationResult Success() => new(true, Array.Empty<ValidationError>());

    public static OperationResult Failure(IReadOnlyCollection<ValidationError> errors) => new(false, errors);
}
