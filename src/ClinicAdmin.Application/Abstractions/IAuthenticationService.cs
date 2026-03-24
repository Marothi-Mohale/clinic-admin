using ClinicAdmin.Application.Authentication;

namespace ClinicAdmin.Application.Abstractions;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> RegisterAccountAsync(
        string fileNumber,
        string username,
        string password,
        string idNumber,
        string email,
        string confirmedIdNumber,
        string confirmedEmail,
        CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}

