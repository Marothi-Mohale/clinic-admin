namespace ClinicAdmin.Application.Common.Validation;

public interface IValidator<in T>
{
    Task<IReadOnlyCollection<ValidationError>> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
