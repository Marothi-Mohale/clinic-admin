using ClinicAdmin.Application.Abstractions;

namespace ClinicAdmin.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
