using ClinicAdmin.Application.Abstractions;

namespace ClinicAdmin.Infrastructure.Clock;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
