using System.Collections.ObjectModel;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class PatientProfileViewModel : ViewModelBase
{
    private string _patientNumber = string.Empty;
    private string _displayName = "No patient selected";
    private string _summary = "Search for a patient to view demographic and file details.";
    private string? _fileNumber;
    private string? _fileStatus;
    private string? _fileLocation;
    private string? _phoneNumber;
    private string? _nationalIdNumber;
    private string? _passportNumber;
    private string? _address;
    private string? _nextOfKin;

    public string PatientNumber
    {
        get => _patientNumber;
        set => SetProperty(ref _patientNumber, value);
    }

    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string Summary
    {
        get => _summary;
        set => SetProperty(ref _summary, value);
    }

    public string? FileNumber
    {
        get => _fileNumber;
        set => SetProperty(ref _fileNumber, value);
    }

    public string? FileStatus
    {
        get => _fileStatus;
        set => SetProperty(ref _fileStatus, value);
    }

    public string? FileLocation
    {
        get => _fileLocation;
        set => SetProperty(ref _fileLocation, value);
    }

    public string? PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string? NationalIdNumber
    {
        get => _nationalIdNumber;
        set => SetProperty(ref _nationalIdNumber, value);
    }

    public string? PassportNumber
    {
        get => _passportNumber;
        set => SetProperty(ref _passportNumber, value);
    }

    public string? Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public string? NextOfKin
    {
        get => _nextOfKin;
        set => SetProperty(ref _nextOfKin, value);
    }

    public ObservableCollection<PatientHistoryItemViewModel> History { get; } = new();

    public void Reset()
    {
        PatientNumber = string.Empty;
        DisplayName = "No patient selected";
        Summary = "Search for a patient to view demographic and file details.";
        FileNumber = null;
        FileStatus = null;
        FileLocation = null;
        PhoneNumber = null;
        NationalIdNumber = null;
        PassportNumber = null;
        Address = null;
        NextOfKin = null;
        History.Clear();
    }
}
