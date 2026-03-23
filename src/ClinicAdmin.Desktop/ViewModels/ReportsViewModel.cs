using System.Collections.ObjectModel;
using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Reports.Queries;
using ClinicAdmin.Contracts.Reports;
using ClinicAdmin.Desktop.Commands;
using ClinicAdmin.Desktop.Services;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class ReportsViewModel : ViewModelBase
{
    private readonly IReportingService _reportingService;
    private readonly IReportExportService _reportExportService;
    private readonly IFacilityContext _facilityContext;
    private DateTime _fromDate;
    private DateTime _toDate;
    private string _statusMessage = "Load a date range to review daily registrations, visit activity, staff activity, and patient history summaries.";
    private bool _isBusy;
    private int _totalRegistrations;
    private int _totalVisits;

    public ReportsViewModel(
        IReportingService reportingService,
        IReportExportService reportExportService,
        IFacilityContext facilityContext)
    {
        _reportingService = reportingService;
        _reportExportService = reportExportService;
        _facilityContext = facilityContext;
        _toDate = DateTime.Today;
        _fromDate = _toDate.AddDays(-6);
        DailyRegistrations = new ObservableCollection<DailyRegistrationReportItemDto>();
        DailyVisits = new ObservableCollection<DailyVisitCountReportItemDto>();
        CommonReasons = new ObservableCollection<VisitReasonReportItemDto>();
        StaffActivity = new ObservableCollection<StaffActivityReportItemDto>();
        PatientVisitHistory = new ObservableCollection<PatientVisitHistorySummaryReportItemDto>();
        LoadReportCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        ExportCsvCommand = new AsyncRelayCommand(ExportCsvAsync, () => !IsBusy && HasReportData);
        ResetFiltersCommand = new RelayCommand(ResetFilters, () => !IsBusy);
    }

    public ObservableCollection<DailyRegistrationReportItemDto> DailyRegistrations { get; }
    public ObservableCollection<DailyVisitCountReportItemDto> DailyVisits { get; }
    public ObservableCollection<VisitReasonReportItemDto> CommonReasons { get; }
    public ObservableCollection<StaffActivityReportItemDto> StaffActivity { get; }
    public ObservableCollection<PatientVisitHistorySummaryReportItemDto> PatientVisitHistory { get; }

    public AsyncRelayCommand LoadReportCommand { get; }
    public AsyncRelayCommand ExportCsvCommand { get; }
    public RelayCommand ResetFiltersCommand { get; }

    public DateTime FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
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
                LoadReportCommand.RaiseCanExecuteChanged();
                ExportCsvCommand.RaiseCanExecuteChanged();
                ResetFiltersCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int TotalRegistrations
    {
        get => _totalRegistrations;
        private set => SetProperty(ref _totalRegistrations, value);
    }

    public int TotalVisits
    {
        get => _totalVisits;
        private set => SetProperty(ref _totalVisits, value);
    }

    public bool HasReportData =>
        DailyRegistrations.Count > 0 ||
        DailyVisits.Count > 0 ||
        CommonReasons.Count > 0 ||
        StaffActivity.Count > 0 ||
        PatientVisitHistory.Count > 0;

    public async Task InitializeAsync()
    {
        if (!HasReportData)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        if (ToDate.Date < FromDate.Date)
        {
            StatusMessage = "The end date must be the same as or later than the start date.";
            return;
        }

        IsBusy = true;
        try
        {
            var report = await _reportingService.GetOperationalReportAsync(
                new ReportQueryDto(
                    _facilityContext.CurrentFacilityId,
                    DateOnly.FromDateTime(FromDate.Date),
                    DateOnly.FromDateTime(ToDate.Date)));

            ReplaceItems(DailyRegistrations, report.DailyRegistrations);
            ReplaceItems(DailyVisits, report.DailyVisits);
            ReplaceItems(CommonReasons, report.CommonReasons);
            ReplaceItems(StaffActivity, report.StaffActivity);
            ReplaceItems(PatientVisitHistory, report.PatientVisitHistory);
            TotalRegistrations = report.TotalRegistrations;
            TotalVisits = report.TotalVisits;
            StatusMessage = "Operational report loaded.";
            RaisePropertyChanged(nameof(HasReportData));
        }
        catch (Exception)
        {
            ClearCollections();
            TotalRegistrations = 0;
            TotalVisits = 0;
            StatusMessage = "The report could not be loaded right now. Please retry or contact an administrator if the problem continues.";
            RaisePropertyChanged(nameof(HasReportData));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportCsvAsync()
    {
        if (!HasReportData)
        {
            StatusMessage = "Load a report before exporting.";
            return;
        }

        IsBusy = true;
        try
        {
            var report = new ClinicOperationalReportDto(
                DateOnly.FromDateTime(FromDate.Date),
                DateOnly.FromDateTime(ToDate.Date),
                TotalRegistrations,
                TotalVisits,
                DailyRegistrations.ToArray(),
                DailyVisits.ToArray(),
                CommonReasons.ToArray(),
                StaffActivity.ToArray(),
                PatientVisitHistory.ToArray());

            var path = await _reportExportService.ExportOperationalReportCsvAsync(report);
            StatusMessage = $"Report exported to {path}";
        }
        catch (Exception)
        {
            StatusMessage = "The report could not be exported right now.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetFilters()
    {
        ToDate = DateTime.Today;
        FromDate = ToDate.AddDays(-6);
        ClearCollections();
        TotalRegistrations = 0;
        TotalVisits = 0;
        StatusMessage = "Load a date range to review daily registrations, visit activity, staff activity, and patient history summaries.";
        RaisePropertyChanged(nameof(HasReportData));
    }

    private void ClearCollections()
    {
        DailyRegistrations.Clear();
        DailyVisits.Clear();
        CommonReasons.Clear();
        StaffActivity.Clear();
        PatientVisitHistory.Clear();
    }

    private static void ReplaceItems<T>(ObservableCollection<T> collection, IEnumerable<T> items)
    {
        collection.Clear();
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}
