using System.Collections.ObjectModel;
using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Common.Exceptions;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Application.Visits.Commands.RegisterVisit;
using ClinicAdmin.Contracts.Visits;
using ClinicAdmin.Desktop.Commands;
using ClinicAdmin.Domain.Visits;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class VisitCaptureViewModel : ViewModelBase
{
    private readonly IPatientSearchService _patientSearchService;
    private readonly IVisitWorkflowService _visitWorkflowService;
    private readonly IFacilityContext _facilityContext;
    private string _patientSearchTerm = string.Empty;
    private bool _isBusy;
    private string _reasonForVisit = string.Empty;
    private QueueStatus _selectedQueueStatus = QueueStatus.Waiting;
    private VisitState _selectedVisitState = VisitState.Registered;
    private string _department = string.Empty;
    private string _assignedStaffMember = string.Empty;
    private string _notes = string.Empty;
    private string _statusMessage = "Search for the patient first, then capture the arrival details.";
    private PatientSearchResultItemViewModel? _selectedPatient;
    private VisitHistoryItemViewModel? _selectedVisit;

    public VisitCaptureViewModel(
        IPatientSearchService patientSearchService,
        IVisitWorkflowService visitWorkflowService,
        IFacilityContext facilityContext)
    {
        _patientSearchService = patientSearchService;
        _visitWorkflowService = visitWorkflowService;
        _facilityContext = facilityContext;
        PatientResults = new ObservableCollection<PatientSearchResultItemViewModel>();
        VisitHistory = new ObservableCollection<VisitHistoryItemViewModel>();
        QueueStatusOptions = Enum.GetValues<QueueStatus>();
        VisitStateOptions = Enum.GetValues<VisitState>();

        SearchPatientsCommand = new AsyncRelayCommand(SearchPatientsAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(PatientSearchTerm));
        RegisterVisitCommand = new AsyncRelayCommand(RegisterVisitAsync, CanSubmitVisit);
        UpdateVisitCommand = new AsyncRelayCommand(UpdateVisitAsync, () => !IsBusy && SelectedVisit is not null);
        ClearVisitFormCommand = new RelayCommand(ClearVisitForm, () => !IsBusy);
    }

    public ObservableCollection<PatientSearchResultItemViewModel> PatientResults { get; }

    public ObservableCollection<VisitHistoryItemViewModel> VisitHistory { get; }

    public IReadOnlyCollection<QueueStatus> QueueStatusOptions { get; }

    public IReadOnlyCollection<VisitState> VisitStateOptions { get; }

    public AsyncRelayCommand SearchPatientsCommand { get; }

    public AsyncRelayCommand RegisterVisitCommand { get; }

    public AsyncRelayCommand UpdateVisitCommand { get; }

    public RelayCommand ClearVisitFormCommand { get; }

    public string PatientSearchTerm
    {
        get => _patientSearchTerm;
        set
        {
            if (SetProperty(ref _patientSearchTerm, value))
            {
                SearchPatientsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public PatientSearchResultItemViewModel? SelectedPatient
    {
        get => _selectedPatient;
        set
        {
            if (SetProperty(ref _selectedPatient, value))
            {
                RegisterVisitCommand.RaiseCanExecuteChanged();
                _ = LoadVisitHistoryAsync();
            }
        }
    }

    public VisitHistoryItemViewModel? SelectedVisit
    {
        get => _selectedVisit;
        set
        {
            if (SetProperty(ref _selectedVisit, value))
            {
                UpdateVisitCommand.RaiseCanExecuteChanged();
                if (value is not null)
                {
                    SelectedQueueStatus = Enum.Parse<QueueStatus>(value.QueueStatus);
                    SelectedVisitState = Enum.Parse<VisitState>(value.State);
                    Department = value.Department ?? string.Empty;
                    AssignedStaffMember = value.AssignedStaffMember ?? string.Empty;
                    Notes = value.Notes;
                    ReasonForVisit = value.ReasonForVisit;
                }
            }
        }
    }

    public string ReasonForVisit
    {
        get => _reasonForVisit;
        set
        {
            if (SetProperty(ref _reasonForVisit, value))
            {
                RegisterVisitCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public QueueStatus SelectedQueueStatus
    {
        get => _selectedQueueStatus;
        set => SetProperty(ref _selectedQueueStatus, value);
    }

    public VisitState SelectedVisitState
    {
        get => _selectedVisitState;
        set => SetProperty(ref _selectedVisitState, value);
    }

    public string Department
    {
        get => _department;
        set => SetProperty(ref _department, value);
    }

    public string AssignedStaffMember
    {
        get => _assignedStaffMember;
        set => SetProperty(ref _assignedStaffMember, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
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
                SearchPatientsCommand.RaiseCanExecuteChanged();
                RegisterVisitCommand.RaiseCanExecuteChanged();
                UpdateVisitCommand.RaiseCanExecuteChanged();
                ClearVisitFormCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private bool CanSubmitVisit() =>
        !IsBusy &&
        SelectedPatient is not null &&
        !string.IsNullOrWhiteSpace(ReasonForVisit);

    private async Task SearchPatientsAsync()
    {
        PatientSearchResultItemViewModel? firstResult = null;

        await ExecuteBusyAsync(async () =>
        {
            PatientResults.Clear();
            SelectedPatient = null;
            VisitHistory.Clear();

            var results = await _patientSearchService.SearchAsync(new SearchPatientsQuery(_facilityContext.CurrentFacilityId, PatientSearchTerm, 0, 15));
            foreach (var result in results)
            {
                PatientResults.Add(new PatientSearchResultItemViewModel
                {
                    Id = result.Id,
                    PatientNumber = result.PatientNumber,
                    DisplayName = result.DisplayName,
                    DateOfBirth = result.DateOfBirth,
                    NationalIdNumber = result.NationalIdNumber,
                    PassportNumber = result.PassportNumber,
                    PhoneNumber = result.PhoneNumber,
                    FileNumber = result.FileNumber,
                    FileStatus = result.FileStatus,
                    FileLocation = result.FileLocation
                });
            }

            if (results.Count == 0)
            {
                StatusMessage = $"No patients were found for \"{PatientSearchTerm}\".";
                return;
            }

            firstResult = PatientResults[0];
            StatusMessage = $"{results.Count} patient(s) found. Select the patient and capture the visit.";
        });

        if (firstResult is not null)
        {
            SelectedPatient = firstResult;
        }
    }

    private async Task RegisterVisitAsync()
    {
        if (SelectedPatient is null)
        {
            return;
        }

        var shouldRefreshHistory = false;

        await ExecuteBusyAsync(async () =>
        {
            try
            {
                var visit = await _visitWorkflowService.RegisterArrivalAsync(new RegisterVisitCommand(
                    _facilityContext.CurrentFacilityId,
                    SelectedPatient.Id,
                    ReasonForVisit,
                    SelectedQueueStatus,
                    SelectedVisitState,
                    EmptyToNull(Department),
                    EmptyToNull(AssignedStaffMember),
                    EmptyToNull(Notes)));

                StatusMessage = $"Visit registered for {visit.PatientDisplayName}. Queue status: {visit.QueueStatus}.";
                ClearVisitForm();
                shouldRefreshHistory = true;
            }
            catch (ValidationException validationException)
            {
                StatusMessage = string.Join(Environment.NewLine, validationException.Errors.Select(x => x.ErrorMessage));
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
            }
        });

        if (shouldRefreshHistory)
        {
            await LoadVisitHistoryAsync();
        }
    }

    private async Task UpdateVisitAsync()
    {
        if (SelectedVisit is null)
        {
            return;
        }

        var visitId = SelectedVisit.Id;
        var shouldRefreshHistory = false;

        await ExecuteBusyAsync(async () =>
        {
            try
            {
                var visit = await _visitWorkflowService.UpdateVisitAsync(new UpdateVisitStateCommand(
                    _facilityContext.CurrentFacilityId,
                    visitId,
                    SelectedQueueStatus,
                    SelectedVisitState,
                    EmptyToNull(Department),
                    EmptyToNull(AssignedStaffMember),
                    EmptyToNull(Notes)));

                StatusMessage = $"Visit updated to {visit.State} / {visit.QueueStatus}.";
                shouldRefreshHistory = true;
            }
            catch (ValidationException validationException)
            {
                StatusMessage = string.Join(Environment.NewLine, validationException.Errors.Select(x => x.ErrorMessage));
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
            }
        });

        if (shouldRefreshHistory)
        {
            await LoadVisitHistoryAsync();
            SelectedVisit = VisitHistory.FirstOrDefault(x => x.Id == visitId);
        }
    }

    private async Task LoadVisitHistoryAsync()
    {
        if (SelectedPatient is null || IsBusy)
        {
            return;
        }

        await ExecuteBusyAsync(async () =>
        {
            VisitHistory.Clear();
            var history = await _visitWorkflowService.GetVisitHistoryAsync(_facilityContext.CurrentFacilityId, SelectedPatient.Id, 20);
            foreach (var item in history)
            {
                VisitHistory.Add(new VisitHistoryItemViewModel
                {
                    Id = item.Id,
                    ArrivedAtUtc = item.ArrivedAtUtc,
                    ReasonForVisit = item.ReasonForVisit,
                    QueueStatus = item.QueueStatus,
                    State = item.State,
                    Department = item.Department,
                    AssignedStaffMember = item.AssignedStaffMember,
                    Notes = item.Notes
                });
            }
        });
    }

    private void ClearVisitForm()
    {
        SelectedVisit = null;
        ReasonForVisit = string.Empty;
        SelectedQueueStatus = QueueStatus.Waiting;
        SelectedVisitState = VisitState.Registered;
        Department = string.Empty;
        AssignedStaffMember = string.Empty;
        Notes = string.Empty;
    }

    private async Task ExecuteBusyAsync(Func<Task> action)
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

    private static string? EmptyToNull(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
