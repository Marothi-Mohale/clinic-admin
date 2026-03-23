namespace ClinicAdmin.Application.Abstractions;

public interface IAuditService
{
    Task WriteAsync(string action, string entityName, Guid entityId, string details, CancellationToken cancellationToken = default);
    Task WriteAuthenticationAsync(string attemptedUsername, bool succeeded, string details, CancellationToken cancellationToken = default);
    Task WriteChangeAsync(
        string action,
        string entityName,
        Guid? entityId,
        string details,
        string? beforeSummary = null,
        string? afterSummary = null,
        string? metadata = null,
        bool succeeded = true,
        CancellationToken cancellationToken = default);
}
