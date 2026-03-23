using ClinicAdmin.Application.Authentication;

namespace ClinicAdmin.Application.Abstractions;

public interface IUserSessionService
{
    UserSession? CurrentSession { get; }
    bool IsAuthenticated { get; }
    event EventHandler<UserSession?>? SessionChanged;

    void SetSession(UserSession session);
    void ClearSession();
}

