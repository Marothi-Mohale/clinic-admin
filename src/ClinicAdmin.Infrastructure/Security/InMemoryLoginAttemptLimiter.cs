using System.Collections.Concurrent;
using ClinicAdmin.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClinicAdmin.Infrastructure.Security;

public sealed class InMemoryLoginAttemptLimiter : ILoginAttemptLimiter
{
    private readonly ConcurrentDictionary<string, FailureState> _failures = new(StringComparer.OrdinalIgnoreCase);
    private readonly AuthenticationOptions _options;

    public InMemoryLoginAttemptLimiter(IOptions<AuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public bool IsLockedOut(string username, DateTimeOffset now, out DateTimeOffset? lockedUntilUtc)
    {
        lockedUntilUtc = null;
        if (!_failures.TryGetValue(username, out var state))
        {
            return false;
        }

        if (state.LockedUntilUtc is null)
        {
            return false;
        }

        if (state.LockedUntilUtc <= now)
        {
            _failures.TryRemove(username, out _);
            return false;
        }

        lockedUntilUtc = state.LockedUntilUtc;
        return true;
    }

    public void RecordFailure(string username, DateTimeOffset now)
    {
        _failures.AddOrUpdate(
            username,
            _ => CreateInitialState(now),
            (_, existing) => UpdateState(existing, now));
    }

    public void ClearFailures(string username)
    {
        _failures.TryRemove(username, out _);
    }

    private FailureState CreateInitialState(DateTimeOffset now)
    {
        var count = 1;
        DateTimeOffset? lockedUntilUtc = count >= _options.MaxFailedAttempts
            ? now.AddMinutes(_options.LockoutDurationMinutes)
            : null;

        return new FailureState(count, lockedUntilUtc);
    }

    private FailureState UpdateState(FailureState existing, DateTimeOffset now)
    {
        var count = existing.Count + 1;
        DateTimeOffset? lockedUntilUtc = count >= _options.MaxFailedAttempts
            ? now.AddMinutes(_options.LockoutDurationMinutes)
            : existing.LockedUntilUtc;

        return new FailureState(count, lockedUntilUtc);
    }

    private sealed record FailureState(int Count, DateTimeOffset? LockedUntilUtc);
}
