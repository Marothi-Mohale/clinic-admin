namespace ClinicAdmin.Infrastructure.Security;

public interface ILoginAttemptLimiter
{
    bool IsLockedOut(string username, DateTimeOffset now, out DateTimeOffset? lockedUntilUtc);

    void RecordFailure(string username, DateTimeOffset now);

    void ClearFailures(string username);
}
