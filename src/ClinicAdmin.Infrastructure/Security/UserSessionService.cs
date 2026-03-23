using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;

namespace ClinicAdmin.Infrastructure.Security;

public sealed class UserSessionService : ICurrentUserService, IUserSessionService
{
    private readonly object _lock = new();
    private UserSession? _currentSession;

    public UserSession? CurrentSession
    {
        get
        {
            lock (_lock)
            {
                return _currentSession;
            }
        }
    }

    public bool IsAuthenticated => CurrentSession is not null;

    public string Username => CurrentSession?.Username ?? string.Empty;

    public IReadOnlyCollection<string> Roles =>
        CurrentSession is null
            ? Array.Empty<string>()
            : new[] { CurrentSession.Role.ToString() };

    public event EventHandler<UserSession?>? SessionChanged;

    public void SetSession(UserSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        lock (_lock)
        {
            _currentSession = session;
        }

        SessionChanged?.Invoke(this, session);
    }

    public void ClearSession()
    {
        lock (_lock)
        {
            _currentSession = null;
        }

        SessionChanged?.Invoke(this, null);
    }
}

