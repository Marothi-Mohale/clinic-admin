using System.Collections.ObjectModel;
using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Desktop.Commands;
using ClinicAdmin.Domain.Security;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly IUserSessionService _userSessionService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuthorizationService _authorizationService;
    private bool _isAuthenticated;
    private string _currentUserDisplayName = "Not signed in";
    private string _currentRole = "Guest";
    private string _selectedRoute = "Dashboard";

    public MainWindowViewModel(
        LoginViewModel login,
        IUserSessionService userSessionService,
        IAuthenticationService authenticationService,
        IAuthorizationService authorizationService)
    {
        Login = login;
        _userSessionService = userSessionService;
        _authenticationService = authenticationService;
        _authorizationService = authorizationService;
        NavigationItems = new ObservableCollection<NavigationItemViewModel>();
        LogoutCommand = new AsyncRelayCommand(LogoutAsync, () => IsAuthenticated);

        _userSessionService.SessionChanged += OnSessionChanged;
        ApplySession(_userSessionService.CurrentSession);
    }

    public string Title => "Clinic Administration";

    public string Subtitle => "Fast registration, secure access, and role-based clinic workflows";

    public LoginViewModel Login { get; }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public AsyncRelayCommand LogoutCommand { get; }

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set
        {
            if (SetProperty(ref _isAuthenticated, value))
            {
                RaisePropertyChanged(nameof(IsLoginVisible));
                RaisePropertyChanged(nameof(IsShellVisible));
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsLoginVisible => !IsAuthenticated;

    public bool IsShellVisible => IsAuthenticated;

    public string CurrentUserDisplayName
    {
        get => _currentUserDisplayName;
        private set => SetProperty(ref _currentUserDisplayName, value);
    }

    public string CurrentRole
    {
        get => _currentRole;
        private set => SetProperty(ref _currentRole, value);
    }

    public string SelectedRoute
    {
        get => _selectedRoute;
        set
        {
            if (SetProperty(ref _selectedRoute, value))
            {
                RaisePropertyChanged(nameof(CurrentRouteDescription));
            }
        }
    }

    public string CurrentRouteDescription =>
        NavigationItems.FirstOrDefault(x => x.Route == SelectedRoute)?.Description ??
        "Choose a task from the navigation menu.";

    private async Task LogoutAsync()
    {
        await _authenticationService.LogoutAsync();
        Login.Username = string.Empty;
        Login.Password = string.Empty;
    }

    private void OnSessionChanged(object? sender, UserSession? session)
    {
        ApplySession(session);
    }

    private void ApplySession(UserSession? session)
    {
        IsAuthenticated = session is not null;
        CurrentUserDisplayName = session?.DisplayName ?? "Not signed in";
        CurrentRole = session?.Role.ToString() ?? "Guest";

        NavigationItems.Clear();

        if (session is null)
        {
            SelectedRoute = "Dashboard";
            RaisePropertyChanged(nameof(CurrentRouteDescription));
            return;
        }

        foreach (var item in BuildNavigation(session.Role))
        {
            if (_authorizationService.CanAccess(session.Role, item.Route))
            {
                NavigationItems.Add(item);
            }
        }

        SelectedRoute = NavigationItems.FirstOrDefault()?.Route ?? "Dashboard";
        RaisePropertyChanged(nameof(CurrentRouteDescription));
    }

    private static IReadOnlyCollection<NavigationItemViewModel> BuildNavigation(UserRole role) =>
        role switch
        {
            UserRole.Admin => new[]
            {
                new NavigationItemViewModel("Dashboard", "Dashboard", "Operational overview and quick actions."),
                new NavigationItemViewModel("Patients", "Patients", "Register, search, and manage patient records."),
                new NavigationItemViewModel("Visits", "Visits", "Capture and review clinic visits."),
                new NavigationItemViewModel("Reports", "Reports", "Operational and compliance reporting."),
                new NavigationItemViewModel("Administration", "Administration", "Manage users, roles, and clinic settings.")
            },
            UserRole.Receptionist => new[]
            {
                new NavigationItemViewModel("Dashboard", "Dashboard", "High-speed front desk tasks and daily status."),
                new NavigationItemViewModel("Patients", "Patients", "Register and search patient records."),
                new NavigationItemViewModel("Visits", "Visits", "Capture new patient visits."),
                new NavigationItemViewModel("Files", "Files", "Track file issue and return status.")
            },
            UserRole.Nurse => new[]
            {
                new NavigationItemViewModel("Dashboard", "Dashboard", "Quick patient lookups and visit status."),
                new NavigationItemViewModel("Patients", "Patients", "Search patient summaries and demographics."),
                new NavigationItemViewModel("Visits", "Visits", "Review current and historical visits.")
            },
            UserRole.Doctor => new[]
            {
                new NavigationItemViewModel("Dashboard", "Dashboard", "Current patient context and history access."),
                new NavigationItemViewModel("Patients", "Patients", "Search and review patient profiles."),
                new NavigationItemViewModel("History", "History", "View visits, file movement, and audit history.")
            },
            UserRole.Manager => new[]
            {
                new NavigationItemViewModel("Dashboard", "Dashboard", "Clinic performance overview."),
                new NavigationItemViewModel("Reports", "Reports", "Registration, file, and activity reporting."),
                new NavigationItemViewModel("Audit", "Audit", "Review logins and operational audit events.")
            },
            _ => Array.Empty<NavigationItemViewModel>()
        };
}
