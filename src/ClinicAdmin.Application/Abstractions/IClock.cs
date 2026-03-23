namespace ClinicAdmin.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
