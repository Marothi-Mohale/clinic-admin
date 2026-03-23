using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Authorization;
using ClinicAdmin.Application.Auditing;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Application.Reports.Queries;
using ClinicAdmin.Contracts.Auditing;
using ClinicAdmin.Contracts.Patients;
using ClinicAdmin.Contracts.Reports;
using ClinicAdmin.Application.Visits.Commands.RegisterVisit;
using ClinicAdmin.Contracts.Visits;
using ClinicAdmin.Desktop.Services;
using ClinicAdmin.Desktop.ViewModels;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Domain.Patients;

namespace ClinicAdmin.Desktop.Tests.ViewModels;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_WhenSessionIsAuthenticated_ShouldBuildRoleBasedNavigation()
    {
        var sessionService = new FakeUserSessionService();
        sessionService.SetSession(new UserSession(
            Guid.NewGuid(),
            "MANAGER",
            "Clinic Manager",
            UserRole.Manager,
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            DateTimeOffset.UtcNow));

        var viewModel = new MainWindowViewModel(
            new LoginViewModel(new FakeAuthenticationService(sessionService)),
            sessionService,
            new FakeAuthenticationService(sessionService),
            new AuthorizationService(),
            new PatientRegistrationViewModel(
                new FakePatientRegistrationService(),
                new FakeDuplicateWarningService(),
                new FakeFacilityContext()),
            new PatientSearchViewModel(
                new FakePatientSearchService(),
                new FakeFacilityContext()),
            new VisitCaptureViewModel(
                new FakePatientSearchService(),
                new FakeVisitWorkflowService(),
                new FakeFacilityContext()),
            new AuditLogViewModel(
                new FakeAuditLogQueryService(),
                new FakeFacilityContext()),
            new ReportsViewModel(
                new FakeReportingService(),
                new FakeReportExportService(),
                new FakeFacilityContext()));

        Assert.True(viewModel.IsAuthenticated);
        Assert.Contains(viewModel.NavigationItems, item => item.Route == "Reports");
        Assert.Contains(viewModel.NavigationItems, item => item.Route == "Audit");
        Assert.DoesNotContain(viewModel.NavigationItems, item => item.Route == "Administration");
    }

    private sealed class FakePatientRegistrationService : IPatientRegistrationService
    {
        public Task<RegisterPatientCommandResult> RegisterAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(RegisterPatientCommandResult.Success(Guid.NewGuid(), command.PatientNumber));
    }

    private sealed class FakeDuplicateWarningService : IPatientRegistrationDuplicateWarningService
    {
        public Task<DuplicateWarningResult> CheckAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DuplicateWarningResult(ClinicAdmin.Application.Patients.DuplicateDetection.DuplicateActionRecommendation.SafeToCreate, Array.Empty<ClinicAdmin.Contracts.Patients.DuplicatePatientWarningDto>()));
    }

    private sealed class FakePatientSearchService : IPatientSearchService
    {
        public Task<PatientProfileDto?> GetProfileAsync(Guid facilityId, Guid patientId, CancellationToken cancellationToken = default) =>
            Task.FromResult<PatientProfileDto?>(null);

        public Task<IReadOnlyCollection<PatientSearchResultDto>> SearchAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<PatientSearchResultDto>>(Array.Empty<PatientSearchResultDto>());
    }

    private sealed class FakeVisitWorkflowService : IVisitWorkflowService
    {
        public Task<IReadOnlyCollection<VisitHistoryItemDto>> GetVisitHistoryAsync(Guid facilityId, Guid patientId, int take = 20, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VisitHistoryItemDto>>(Array.Empty<VisitHistoryItemDto>());

        public Task<VisitSummaryDto> RegisterArrivalAsync(RegisterVisitCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VisitSummaryDto(Guid.NewGuid(), command.PatientId, "P-100", "Patient", DateTimeOffset.UtcNow, command.ReasonForVisit, command.QueueStatus.ToString(), command.State.ToString(), command.Department, command.AssignedStaffMember, command.Notes ?? string.Empty));

        public Task<VisitSummaryDto> UpdateVisitAsync(UpdateVisitStateCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VisitSummaryDto(command.VisitId, Guid.NewGuid(), "P-100", "Patient", DateTimeOffset.UtcNow, "Review", command.QueueStatus.ToString(), command.State.ToString(), command.Department, command.AssignedStaffMember, command.Notes ?? string.Empty));
    }

    private sealed class FakeAuditLogQueryService : IAuditLogQueryService
    {
        public Task<IReadOnlyCollection<AuditLogItemDto>> QueryAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<AuditLogItemDto>>(Array.Empty<AuditLogItemDto>());
    }

    private sealed class FakeReportingService : IReportingService
    {
        public Task<ClinicOperationalReportDto> GetOperationalReportAsync(ReportQueryDto query, CancellationToken cancellationToken = default) =>
            Task.FromResult(new ClinicOperationalReportDto(
                query.FromDate,
                query.ToDate,
                0,
                0,
                Array.Empty<DailyRegistrationReportItemDto>(),
                Array.Empty<DailyVisitCountReportItemDto>(),
                Array.Empty<VisitReasonReportItemDto>(),
                Array.Empty<StaffActivityReportItemDto>(),
                Array.Empty<PatientVisitHistorySummaryReportItemDto>()));
    }

    private sealed class FakeReportExportService : IReportExportService
    {
        public Task<string> ExportOperationalReportCsvAsync(ClinicOperationalReportDto report, CancellationToken cancellationToken = default) =>
            Task.FromResult(@"C:\Exports\clinic-report.csv");
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        private readonly IUserSessionService _sessionService;

        public FakeAuthenticationService(IUserSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default) =>
            Task.FromResult(AuthenticationResult.Failure(AuthenticationErrorCode.InvalidCredentials, "Not implemented for this test."));

        public Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            _sessionService.ClearSession();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserSessionService : IUserSessionService
    {
        private UserSession? _currentSession;

        public UserSession? CurrentSession => _currentSession;
        public bool IsAuthenticated => _currentSession is not null;
        public event EventHandler<UserSession?>? SessionChanged;

        public void SetSession(UserSession session)
        {
            _currentSession = session;
            SessionChanged?.Invoke(this, session);
        }

        public void ClearSession()
        {
            _currentSession = null;
            SessionChanged?.Invoke(this, null);
        }
    }
}
