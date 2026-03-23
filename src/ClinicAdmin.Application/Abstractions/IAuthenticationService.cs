using ClinicAdmin.Application.Authentication;

namespace ClinicAdmin.Application.Abstractions;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}

