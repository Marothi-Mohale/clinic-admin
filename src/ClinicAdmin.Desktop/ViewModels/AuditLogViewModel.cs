using System.Collections.ObjectModel;
using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Auditing;
using ClinicAdmin.Contracts.Auditing;
using ClinicAdmin.Desktop.Commands;

namespace ClinicAdmin.Desktop.ViewModels;

public sealed class AuditLogViewModel : ViewModelBase
{
    private readonly IAuditLogQueryService _auditLogQueryService;
    private readonly IFacilityContext _facilityContext;
    private string _searchTerm = string.Empty;
    private string _actionFilter = string.Empty;
    private string _entityFilter = string.Empty;
    private string _actorFilter = string.Empty;
    private string _statusMessage = "Use filters to review authentication attempts, registrations, visits, and administrative actions.";
    private bool _isBusy;

    public AuditLogViewModel(IAuditLogQueryService auditLogQueryService, IFacilityContext facilityContext)
    {
        _auditLogQueryService = auditLogQueryService;
        _facilityContext = facilityContext;
        AuditItems = new ObservableCollection<AuditLogItemViewModel>();
        LoadAuditCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        ClearFiltersCommand = new RelayCommand(ClearFilters, () => !IsBusy);
    }

    public ObservableCollection<AuditLogItemViewModel> AuditItems { get; }

    public AsyncRelayCommand LoadAuditCommand { get; }

    public RelayCommand ClearFiltersCommand { get; }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public string ActionFilter
    {
        get => _actionFilter;
        set => SetProperty(ref _actionFilter, value);
    }

    public string EntityFilter
    {
        get => _entityFilter;
        set => SetProperty(ref _entityFilter, value);
    }

    public string ActorFilter
    {
        get => _actorFilter;
        set => SetProperty(ref _actorFilter, value);
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
                LoadAuditCommand.RaiseCanExecuteChanged();
                ClearFiltersCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public async Task InitializeAsync()
    {
        if (AuditItems.Count == 0)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            AuditItems.Clear();

            var results = await _auditLogQueryService.QueryAsync(new AuditLogQueryDto(
                _facilityContext.CurrentFacilityId,
                string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                string.IsNullOrWhiteSpace(ActionFilter) ? null : ActionFilter,
                string.IsNullOrWhiteSpace(EntityFilter) ? null : EntityFilter,
                string.IsNullOrWhiteSpace(ActorFilter) ? null : ActorFilter,
                DateTimeOffset.UtcNow.AddDays(-14),
                null,
                150));

            foreach (var result in results)
            {
                AuditItems.Add(new AuditLogItemViewModel
                {
                    Id = result.Id,
                    OccurredAtUtc = result.OccurredAtUtc,
                    ActorUsername = result.ActorUsername,
                    Action = result.Action,
                    EntityName = result.EntityName,
                    EntityId = result.EntityId,
                    Details = result.Details,
                    BeforeSummary = result.BeforeSummary,
                    AfterSummary = result.AfterSummary,
                    Metadata = result.Metadata,
                    Workstation = result.Workstation,
                    Succeeded = result.Succeeded
                });
            }

            StatusMessage = results.Count == 0
                ? "No audit entries matched the current filters."
                : $"{results.Count} audit entries loaded.";
        }
        catch (Exception)
        {
            AuditItems.Clear();
            StatusMessage = "Audit entries could not be loaded right now. Please retry or contact an administrator if the problem continues.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearFilters()
    {
        SearchTerm = string.Empty;
        ActionFilter = string.Empty;
        EntityFilter = string.Empty;
        ActorFilter = string.Empty;
        AuditItems.Clear();
        StatusMessage = "Use filters to review authentication attempts, registrations, visits, and administrative actions.";
    }
}
