using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Authorization;
using ClinicAdmin.Desktop.ViewModels;
using ClinicAdmin.Domain.Security;

namespace ClinicAdmin.Desktop.Tests.ViewModels;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_WhenSessionIsAuthenticated_ShouldBuildRoleBasedNavigation()
    {
        var sessionService = new FakeUserSessionService();
        sessionService.SetSession(new UserSession(
            Guid.NewGuid(),
            "MANAGER",
            "Clinic Manager",
            UserRole.Manager,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            DateTimeOffset.UtcNow));

        var viewModel = new MainWindowViewModel(
            new LoginViewModel(new FakeAuthenticationService(sessionService)),
            sessionService,
            new FakeAuthenticationService(sessionService),
            new AuthorizationService());

        Assert.True(viewModel.IsAuthenticated);
        Assert.Contains(viewModel.NavigationItems, item => item.Route == "Reports");
        Assert.DoesNotContain(viewModel.NavigationItems, item => item.Route == "Administration");
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        private readonly IUserSessionService _sessionService;

        public FakeAuthenticationService(IUserSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult(AuthenticationResult.Failure(AuthenticationErrorCode.InvalidCredentials, "Not implemented for this test."));

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            _sessionService.ClearSession();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserSessionService : IUserSessionService
    {
        private UserSession? _currentSession;

        public UserSession? CurrentSession => _currentSession;
        public bool IsAuthenticated => _currentSession is not null;
        public event EventHandler<UserSession?>? SessionChanged;

        public void SetSession(UserSession session)
        {
            _currentSession = session;
            SessionChanged?.Invoke(this, session);
        }

        public void ClearSession()
        {
            _currentSession = null;
            SessionChanged?.Invoke(this, null);
        }
    }
}
