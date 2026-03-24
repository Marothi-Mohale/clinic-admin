using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Desktop.Commands;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _registerFileNumber = string.Empty;
    private string _registerUsername = string.Empty;
    private string _registerPassword = string.Empty;
    private string _registerIdNumber = string.Empty;
    private string _registerEmail = string.Empty;
    private string _registerConfirmedIdNumber = string.Empty;
    private string _registerConfirmedEmail = string.Empty;
    private bool _isRegistrationMode;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        RegisterCommand = new AsyncRelayCommand(RegisterAsync, CanRegister);
        OpenRegistrationCommand = new RelayCommand(OpenRegistrationMode);
        CancelRegistrationCommand = new RelayCommand(CancelRegistrationMode);
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
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
                StatusMessage = string.Empty;
                LoginCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegisterFileNumber
    {
        get => _registerFileNumber;
        set
        {
            if (SetProperty(ref _registerFileNumber, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegisterUsername
    {
        get => _registerUsername;
        set
        {
            if (SetProperty(ref _registerUsername, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegisterPassword
    {
        get => _registerPassword;
        set
        {
            if (SetProperty(ref _registerPassword, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegisterIdNumber
    {
        get => _registerIdNumber;
        set
        {
            if (SetProperty(ref _registerIdNumber, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegisterEmail
    {
        get => _registerEmail;
        set
        {
            if (SetProperty(ref _registerEmail, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegisterConfirmedIdNumber
    {
        get => _registerConfirmedIdNumber;
        set
        {
            if (SetProperty(ref _registerConfirmedIdNumber, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string RegisterConfirmedEmail
    {
        get => _registerConfirmedEmail;
        set
        {
            if (SetProperty(ref _registerConfirmedEmail, value))
            {
                ErrorMessage = string.Empty;
                StatusMessage = string.Empty;
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsRegistrationMode
    {
        get => _isRegistrationMode;
        private set => SetProperty(ref _isRegistrationMode, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                LoginCommand.RaiseCanExecuteChanged();
                RegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand LoginCommand { get; }
    public AsyncRelayCommand RegisterCommand { get; }
    public RelayCommand OpenRegistrationCommand { get; }
    public RelayCommand CancelRegistrationCommand { get; }

    private bool CanLogin() =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    private bool CanRegister() =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(RegisterFileNumber) &&
        !string.IsNullOrWhiteSpace(RegisterUsername) &&
        !string.IsNullOrWhiteSpace(RegisterPassword) &&
        !string.IsNullOrWhiteSpace(RegisterIdNumber) &&
        !string.IsNullOrWhiteSpace(RegisterEmail) &&
        !string.IsNullOrWhiteSpace(RegisterConfirmedIdNumber) &&
        !string.IsNullOrWhiteSpace(RegisterConfirmedEmail);

    private async Task LoginAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _authenticationService.LoginAsync(Username, Password);
            if (!result.Succeeded)
            {
                ErrorMessage = result.Message;
                StatusMessage = string.Empty;
                return;
            }

            ErrorMessage = string.Empty;
            StatusMessage = string.Empty;
            Password = string.Empty;
        }
        catch
        {
            ErrorMessage = "The application could not complete login. Please try again.";
            StatusMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegisterAsync()
    {
        IsBusy = true;

        try
        {
            var result = await _authenticationService.RegisterAccountAsync(
                RegisterFileNumber,
                RegisterUsername,
                RegisterPassword,
                RegisterIdNumber,
                RegisterEmail,
                RegisterConfirmedIdNumber,
                RegisterConfirmedEmail);

            if (!result.Succeeded)
            {
                ErrorMessage = result.Message;
                StatusMessage = string.Empty;
                return;
            }

            ErrorMessage = string.Empty;
            StatusMessage = result.Message;
            Username = RegisterUsername.Trim();
            ClearRegistrationFields();
            IsRegistrationMode = false;
        }
        catch
        {
            ErrorMessage = "The application could not register this account. Please try again.";
            StatusMessage = string.Empty;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenRegistrationMode()
    {
        IsRegistrationMode = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
    }

    private void CancelRegistrationMode()
    {
        IsRegistrationMode = false;
        ClearRegistrationFields();
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
    }

    private void ClearRegistrationFields()
    {
        RegisterFileNumber = string.Empty;
        RegisterUsername = string.Empty;
        RegisterPassword = string.Empty;
        RegisterIdNumber = string.Empty;
        RegisterEmail = string.Empty;
        RegisterConfirmedIdNumber = string.Empty;
        RegisterConfirmedEmail = string.Empty;
    }
}

