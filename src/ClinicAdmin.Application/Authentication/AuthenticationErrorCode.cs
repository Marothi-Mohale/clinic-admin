namespace ClinicAdmin.Application.Authentication;

public enum AuthenticationErrorCode
{
    None = 0,
    ValidationFailed = 1,
    InvalidCredentials = 2,
    UserInactive = 3,
    UnexpectedError = 4,
    LockedOut = 5
}
