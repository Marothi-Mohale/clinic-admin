using System.Collections.ObjectModel;
using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Common.Exceptions;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Contracts.Patients;
using ClinicAdmin.Desktop.Commands;
using ClinicAdmin.Domain.Patients;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class PatientRegistrationViewModel : ViewModelBase
{
    private readonly IPatientRegistrationService _patientRegistrationService;
    private readonly IPatientRegistrationDuplicateWarningService _duplicateWarningService;
    private readonly IFacilityContext _facilityContext;
    private string _patientNumber = string.Empty;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private DateTime? _dateOfBirth;
    private Sex _selectedSex = Sex.Unknown;
    private string _nationalIdNumber = string.Empty;
    private string _passportNumber = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _addressLine1 = string.Empty;
    private string _addressLine2 = string.Empty;
    private string _suburb = string.Empty;
    private string _city = string.Empty;
    private string _nextOfKinName = string.Empty;
    private string _nextOfKinRelationship = string.Empty;
    private string _nextOfKinPhoneNumber = string.Empty;
    private string _statusMessage = "Capture the core details first, then save the patient record.";
    private bool _statusIsError;
    private bool _isBusy;
    private bool _duplicateWarningAcknowledged;

    public PatientRegistrationViewModel(
        IPatientRegistrationService patientRegistrationService,
        IPatientRegistrationDuplicateWarningService duplicateWarningService,
        IFacilityContext facilityContext)
    {
        _patientRegistrationService = patientRegistrationService;
        _duplicateWarningService = duplicateWarningService;
        _facilityContext = facilityContext;

        SexOptions = Enum.GetValues<Sex>()
            .Where(value => value != Sex.Unknown)
            .ToArray();

        DuplicateWarnings = new ObservableCollection<DuplicatePatientWarningItemViewModel>();
        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSubmit);
        CheckDuplicatesCommand = new AsyncRelayCommand(CheckDuplicatesAsync, CanSubmit);
        ClearFormCommand = new RelayCommand(ClearForm, () => !IsBusy);
    }

    public IReadOnlyCollection<Sex> SexOptions { get; }

    public ObservableCollection<DuplicatePatientWarningItemViewModel> DuplicateWarnings { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand CheckDuplicatesCommand { get; }

    public RelayCommand ClearFormCommand { get; }

    public string PatientNumber
    {
        get => _patientNumber;
        set => UpdateField(ref _patientNumber, value);
    }

    public string FirstName
    {
        get => _firstName;
        set => UpdateField(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => UpdateField(ref _lastName, value);
    }

    public DateTime? DateOfBirth
    {
        get => _dateOfBirth;
        set => UpdateField(ref _dateOfBirth, value);
    }

    public Sex SelectedSex
    {
        get => _selectedSex;
        set => UpdateField(ref _selectedSex, value);
    }

    public string NationalIdNumber
    {
        get => _nationalIdNumber;
        set => UpdateField(ref _nationalIdNumber, value);
    }

    public string PassportNumber
    {
        get => _passportNumber;
        set => UpdateField(ref _passportNumber, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => UpdateField(ref _phoneNumber, value);
    }

    public string AddressLine1
    {
        get => _addressLine1;
        set => UpdateField(ref _addressLine1, value);
    }

    public string AddressLine2
    {
        get => _addressLine2;
        set => UpdateField(ref _addressLine2, value);
    }

    public string Suburb
    {
        get => _suburb;
        set => UpdateField(ref _suburb, value);
    }

    public string City
    {
        get => _city;
        set => UpdateField(ref _city, value);
    }

    public string NextOfKinName
    {
        get => _nextOfKinName;
        set => UpdateField(ref _nextOfKinName, value);
    }

    public string NextOfKinRelationship
    {
        get => _nextOfKinRelationship;
        set => UpdateField(ref _nextOfKinRelationship, value);
    }

    public string NextOfKinPhoneNumber
    {
        get => _nextOfKinPhoneNumber;
        set => UpdateField(ref _nextOfKinPhoneNumber, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool StatusIsError
    {
        get => _statusIsError;
        private set => SetProperty(ref _statusIsError, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
                CheckDuplicatesCommand.RaiseCanExecuteChanged();
                ClearFormCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool DuplicateWarningAcknowledged
    {
        get => _duplicateWarningAcknowledged;
        set
        {
            if (SetProperty(ref _duplicateWarningAcknowledged, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasDuplicateWarnings => DuplicateWarnings.Count > 0;

    private bool CanSubmit() => !IsBusy && !string.IsNullOrWhiteSpace(PatientNumber) && !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName);

    private async Task CheckDuplicatesAsync()
    {
        await ExecuteBusyActionAsync(async () =>
        {
            var result = await _duplicateWarningService.CheckAsync(BuildCommand());
            ApplyWarnings(result.Warnings);

            if (result.Warnings.Count == 0)
            {
                SetStatus("No likely duplicate matches were found.", false);
                DuplicateWarningAcknowledged = false;
                return;
            }

            SetStatus("Possible duplicate matches were found. Review them before saving.", true);
        });
    }

    private async Task SaveAsync()
    {
        await ExecuteBusyActionAsync(async () =>
        {
            try
            {
                var result = await _patientRegistrationService.RegisterAsync(BuildCommand());
                ApplyWarnings(result.DuplicateWarnings);

                if (!result.Succeeded)
                {
                    DuplicateWarningAcknowledged = result.RequiresConfirmation;
                    SetStatus(result.Message, true);
                    return;
                }

                DuplicateWarningAcknowledged = false;
                SetStatus($"{result.Message} Patient number: {result.PatientNumber}.", false);
                ClearForm(keepStatus: true);
            }
            catch (ValidationException validationException)
            {
                var message = string.Join(Environment.NewLine, validationException.Errors.Select(error => error.ErrorMessage));
                SetStatus(message, true);
            }
            catch
            {
                SetStatus("The patient could not be registered. Please retry and confirm the captured details.", true);
            }
        });
    }

    private async Task ExecuteBusyActionAsync(Func<Task> action)
    {
        IsBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private RegisterPatientCommand BuildCommand() =>
        new(
            _facilityContext.CurrentFacilityId,
            PatientNumber,
            FirstName,
            LastName,
            DateOfBirth.HasValue ? DateOnly.FromDateTime(DateOfBirth.Value) : null,
            SelectedSex,
            NationalIdNumber,
            PassportNumber,
            PhoneNumber,
            AddressLine1,
            AddressLine2,
            Suburb,
            City,
            NextOfKinName,
            NextOfKinRelationship,
            NextOfKinPhoneNumber,
            DuplicateWarningAcknowledged);

    private void ApplyWarnings(IReadOnlyCollection<DuplicatePatientWarningDto> warnings)
    {
        DuplicateWarnings.Clear();
        foreach (var warning in warnings)
        {
            DuplicateWarnings.Add(new DuplicatePatientWarningItemViewModel
            {
                PatientId = warning.PatientId,
                DisplayName = warning.DisplayName,
                PatientNumber = warning.PatientNumber ?? "Unknown",
                DateOfBirth = warning.DateOfBirth,
                NationalIdNumber = warning.NationalIdNumber,
                PassportNumber = warning.PassportNumber,
                PhoneNumber = warning.PhoneNumber,
                Strength = warning.Strength,
                Recommendation = warning.Recommendation,
                Score = warning.Score,
                ReasonSummary = string.Join(", ", warning.Reasons)
            });
        }

        RaisePropertyChanged(nameof(HasDuplicateWarnings));
    }

    private void ClearForm() => ClearForm(keepStatus: false);

    private void ClearForm(bool keepStatus)
    {
        PatientNumber = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        DateOfBirth = null;
        SelectedSex = Sex.Unknown;
        NationalIdNumber = string.Empty;
        PassportNumber = string.Empty;
        PhoneNumber = string.Empty;
        AddressLine1 = string.Empty;
        AddressLine2 = string.Empty;
        Suburb = string.Empty;
        City = string.Empty;
        NextOfKinName = string.Empty;
        NextOfKinRelationship = string.Empty;
        NextOfKinPhoneNumber = string.Empty;
        DuplicateWarningAcknowledged = false;
        DuplicateWarnings.Clear();
        RaisePropertyChanged(nameof(HasDuplicateWarnings));

        if (!keepStatus)
        {
            SetStatus("Capture the core details first, then save the patient record.", false);
        }
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        StatusIsError = isError;
    }

    private void UpdateField<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref field, value, propertyName))
        {
            SaveCommand.RaiseCanExecuteChanged();
            CheckDuplicatesCommand.RaiseCanExecuteChanged();
        }
    }
}
