using ClinicAdmin.Desktop.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace ClinicAdmin.Desktop.Views;

public partial class MainWindow : System.Windows.Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        _viewModel.Login.PropertyChanged += LoginViewModelOnPropertyChanged;
    }

    private void PasswordInput_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Login.Password = PasswordInput.Password;
    }

    private void RegisterPasswordInput_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Login.RegisterPassword = RegisterPasswordInput.Password;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsLoginVisible) && _viewModel.IsLoginVisible)
        {
            PasswordInput.Password = string.Empty;
            RegisterPasswordInput.Password = string.Empty;
        }

    }

    private void LoginViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LoginViewModel.IsRegistrationMode) && !_viewModel.Login.IsRegistrationMode)
        {
            RegisterPasswordInput.Password = string.Empty;
        }
    }
}
