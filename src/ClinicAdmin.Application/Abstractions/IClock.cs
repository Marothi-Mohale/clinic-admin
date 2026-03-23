namespace ClinicAdmin.Application.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
