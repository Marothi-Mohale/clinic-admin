namespace ClinicAdmin.Application.Authentication;

public sealed record AuthenticationResult(
    bool Succeeded,
    AuthenticationErrorCode ErrorCode,
    string Message,
    UserSession? Session = null)
{
    public static AuthenticationResult Success(UserSession session) =>
        new(true, AuthenticationErrorCode.None, "Login successful.", session);

    public static AuthenticationResult Failure(AuthenticationErrorCode errorCode, string message) =>
        new(false, errorCode, message);
}

