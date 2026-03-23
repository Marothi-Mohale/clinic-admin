using System.Collections.ObjectModel;
using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Contracts.Patients;
using ClinicAdmin.Desktop.Commands;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class PatientSearchViewModel : ViewModelBase
{
    private readonly IPatientSearchService _patientSearchService;
    private readonly IFacilityContext _facilityContext;
    private string _searchTerm = string.Empty;
    private string _statusMessage = "Search by patient number, ID, passport, phone number, surname, or full name.";
    private bool _isBusy;
    private PatientSearchResultItemViewModel? _selectedResult;

    public PatientSearchViewModel(IPatientSearchService patientSearchService, IFacilityContext facilityContext)
    {
        _patientSearchService = patientSearchService;
        _facilityContext = facilityContext;
        Results = new ObservableCollection<PatientSearchResultItemViewModel>();
        SelectedProfile = new PatientProfileViewModel();
        SearchCommand = new AsyncRelayCommand(SearchAsync, CanSearch);
        ClearSearchCommand = new RelayCommand(ClearSearch, () => !IsBusy);
        OpenProfileCommand = new AsyncRelayCommand(OpenProfileAsync, () => !IsBusy && SelectedResult is not null);
    }

    public ObservableCollection<PatientSearchResultItemViewModel> Results { get; }

    public PatientProfileViewModel SelectedProfile { get; }

    public AsyncRelayCommand SearchCommand { get; }

    public RelayCommand ClearSearchCommand { get; }

    public AsyncRelayCommand OpenProfileCommand { get; }

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                SearchCommand.RaiseCanExecuteChanged();
            }
        }
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
                SearchCommand.RaiseCanExecuteChanged();
                ClearSearchCommand.RaiseCanExecuteChanged();
                OpenProfileCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public PatientSearchResultItemViewModel? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (SetProperty(ref _selectedResult, value))
            {
                OpenProfileCommand.RaiseCanExecuteChanged();
                _ = LoadSelectedProfileAsync();
            }
        }
    }

    public bool HasResults => Results.Count > 0;

    private bool CanSearch() => !IsBusy && !string.IsNullOrWhiteSpace(SearchTerm);

    private async Task SearchAsync()
    {
        PatientSearchResultItemViewModel? firstResult = null;

        await ExecuteBusyAsync(async () =>
        {
            Results.Clear();
            SelectedResult = null;
            SelectedProfile.Reset();

            var results = await _patientSearchService.SearchAsync(new SearchPatientsQuery(_facilityContext.CurrentFacilityId, SearchTerm, 0, 25));

            foreach (var result in results)
            {
                Results.Add(new PatientSearchResultItemViewModel
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

            RaisePropertyChanged(nameof(HasResults));

            if (results.Count == 0)
            {
                StatusMessage = $"No patients were found for \"{SearchTerm}\". Check the spelling or try a different identifier.";
                return;
            }

            StatusMessage = $"{results.Count} patient result(s) found. Select a result to view the profile summary.";
            firstResult = Results[0];
        });

        if (firstResult is not null)
        {
            SelectedResult = firstResult;
        }
    }

    private async Task OpenProfileAsync()
    {
        await LoadSelectedProfileAsync();
    }

    private async Task LoadSelectedProfileAsync()
    {
        if (SelectedResult is null || IsBusy)
        {
            return;
        }

        await ExecuteBusyAsync(async () =>
        {
            var profile = await _patientSearchService.GetProfileAsync(_facilityContext.CurrentFacilityId, SelectedResult.Id);
            if (profile is null)
            {
                SelectedProfile.Reset();
                StatusMessage = "The selected patient could not be loaded. Please refresh the search results.";
                return;
            }

            SelectedProfile.PatientNumber = profile.PatientNumber;
            SelectedProfile.DisplayName = $"{profile.FirstName} {profile.LastName}";
            SelectedProfile.Summary = $"{profile.Sex} | DOB: {profile.DateOfBirth?.ToString("yyyy-MM-dd") ?? "Unknown"}";
            SelectedProfile.FileNumber = profile.FileNumber;
            SelectedProfile.FileStatus = profile.FileStatus;
            SelectedProfile.FileLocation = profile.FileLocation;
            SelectedProfile.PhoneNumber = profile.PhoneNumber;
            SelectedProfile.NationalIdNumber = profile.NationalIdNumber;
            SelectedProfile.PassportNumber = profile.PassportNumber;
            SelectedProfile.Address = string.Join(", ", new[] { profile.AddressLine1, profile.AddressLine2, profile.Suburb, profile.City }.Where(x => !string.IsNullOrWhiteSpace(x)));
            SelectedProfile.NextOfKin = string.Join(" | ", new[] { profile.NextOfKinName, profile.NextOfKinRelationship, profile.NextOfKinPhoneNumber }.Where(x => !string.IsNullOrWhiteSpace(x)));
            SelectedProfile.History.Clear();

            foreach (var historyItem in profile.History)
            {
                SelectedProfile.History.Add(new PatientHistoryItemViewModel
                {
                    OccurredAtUtc = historyItem.OccurredAtUtc,
                    Action = historyItem.Action,
                    Details = historyItem.Details,
                    Succeeded = historyItem.Succeeded
                });
            }

            StatusMessage = $"Loaded profile for {SelectedProfile.DisplayName}.";
        });
    }

    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        Results.Clear();
        SelectedResult = null;
        SelectedProfile.Reset();
        StatusMessage = "Search by patient number, ID, passport, phone number, surname, or full name.";
        RaisePropertyChanged(nameof(HasResults));
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
}
