namespace ClinicAdmin.Infrastructure.Configuration;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public int MaxFailedAttempts { get; init; } = 5;

    public int LockoutDurationMinutes { get; init; } = 5;
}
