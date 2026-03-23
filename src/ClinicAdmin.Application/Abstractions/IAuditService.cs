namespace ClinicAdmin.Application.Abstractions;

public interface IAuditService
{
    Task WriteAsync(string action, string entityName, Guid entityId, string details, CancellationToken cancellationToken = default);
}

