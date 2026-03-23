using ClinicAdmin.Desktop.ViewModels;

namespace ClinicAdmin.Desktop.Views;

public partial class MainWindow : System.Windows.Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
