using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Desktop.Commands;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ErrorMessage = string.Empty;
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ErrorMessage = string.Empty;
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand LoginCommand { get; }

    private bool CanLogin() =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    private async Task LoginAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _authenticationService.LoginAsync(Username, Password);
            if (!result.Succeeded)
            {
                ErrorMessage = result.Message;
                return;
            }

            ErrorMessage = string.Empty;
            Password = string.Empty;
        }
        catch
        {
            ErrorMessage = "The application could not complete login. Please try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

