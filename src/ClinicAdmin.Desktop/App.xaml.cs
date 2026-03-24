using ClinicAdmin.Application;
using ClinicAdmin.Desktop.Services;
using ClinicAdmin.Desktop.ViewModels;
using ClinicAdmin.Desktop.Views;
using ClinicAdmin.Infrastructure;
using ClinicAdmin.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ClinicAdmin.Desktop;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private IServiceScope? _uiScope;
    private static readonly string? EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.SetBasePath(AppContext.BaseDirectory);
                builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                if (!string.IsNullOrWhiteSpace(EnvironmentName))
                {
                    builder.AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true, reloadOnChange: true);
                }

                builder.AddEnvironmentVariables(prefix: "CLINICADMIN_");
            })
            .UseSerilog((context, _, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.WithProperty("Application", "ClinicAdmin")
                    .Enrich.WithProperty("Environment", EnvironmentName ?? "Default");
            })
            .ConfigureServices((context, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(context.Configuration);
                services.AddSingleton<IReportExportService>(_ => new ReportExportService());
                services.AddScoped<LoginViewModel>();
                services.AddScoped<AuditLogViewModel>();
                services.AddScoped<ReportsViewModel>();
                services.AddScoped<PatientSearchViewModel>();
                services.AddScoped<PatientRegistrationViewModel>();
                services.AddScoped<VisitCaptureViewModel>();
                services.AddScoped<MainWindowViewModel>();
                services.AddScoped<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(System.Windows.StartupEventArgs e)
    {
        try
        {
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<ClinicAdminDbInitializer>();
                await initializer.InitializeAsync();
            }

            _uiScope = _host.Services.CreateScope();
            var mainWindow = _uiScope.ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"The application could not start correctly.\n\n{ex.Message}",
                "Clinic Administration",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);

            Shutdown(-1);
            return;
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(System.Windows.ExitEventArgs e)
    {
        _uiScope?.Dispose();
        await _host.StopAsync();
        _host.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
