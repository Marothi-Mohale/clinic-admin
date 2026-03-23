namespace ClinicAdmin.Desktop.Services;

public interface IUserNotificationService
{
    void ShowError(string message);
    void ShowInformation(string message);
}
