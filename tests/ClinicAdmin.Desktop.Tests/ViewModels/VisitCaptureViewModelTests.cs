using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Application.Visits.Commands.RegisterVisit;
using ClinicAdmin.Contracts.Patients;
using ClinicAdmin.Contracts.Visits;
using ClinicAdmin.Desktop.ViewModels;
using ClinicAdmin.Domain.Visits;

namespace ClinicAdmin.Desktop.Tests.ViewModels;

public sealed class VisitCaptureViewModelTests
{
    [Fact]
    public async Task SearchPatientsCommand_WhenPatientsFound_ShouldPopulateSelectionAndHistory()
    {
        var viewModel = new VisitCaptureViewModel(
            new FakePatientSearchService(),
            new FakeVisitWorkflowService(),
            new FakeFacilityContext())
        {
            PatientSearchTerm = "P-100"
        };

        viewModel.SearchPatientsCommand.Execute(null);
        await Task.Delay(30);

        Assert.NotNull(viewModel.SelectedPatient);
        Assert.NotEmpty(viewModel.VisitHistory);
    }

    private sealed class FakePatientSearchService : IPatientSearchService
    {
        public Task<IReadOnlyCollection<PatientSearchResultDto>> SearchAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<PatientSearchResultDto>>(new[]
            {
                new PatientSearchResultDto(Guid.NewGuid(), "P-100", "Nomsa Dlamini", new DateOnly(1990, 1, 1), "9001011234088", null, "0821234567", "F-12", "Available", "Records room")
            });

        public Task<PatientProfileDto?> GetProfileAsync(Guid facilityId, Guid patientId, CancellationToken cancellationToken = default) =>
            Task.FromResult<PatientProfileDto?>(null);
    }

    private sealed class FakeVisitWorkflowService : IVisitWorkflowService
    {
        public Task<IReadOnlyCollection<VisitHistoryItemDto>> GetVisitHistoryAsync(Guid facilityId, Guid patientId, int take = 20, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VisitHistoryItemDto>>(new[]
            {
                new VisitHistoryItemDto(Guid.NewGuid(), DateTimeOffset.UtcNow, "Acute cough", "Waiting", "Registered", "Outpatients", "Nurse", "Initial arrival")
            });

        public Task<VisitSummaryDto> RegisterArrivalAsync(RegisterVisitCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VisitSummaryDto(Guid.NewGuid(), command.PatientId, "P-100", "Nomsa Dlamini", DateTimeOffset.UtcNow, command.ReasonForVisit, command.QueueStatus.ToString(), command.State.ToString(), command.Department, command.AssignedStaffMember, command.Notes ?? string.Empty));

        public Task<VisitSummaryDto> UpdateVisitAsync(UpdateVisitStateCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VisitSummaryDto(command.VisitId, Guid.NewGuid(), "P-100", "Nomsa Dlamini", DateTimeOffset.UtcNow, "Acute cough", command.QueueStatus.ToString(), command.State.ToString(), command.Department, command.AssignedStaffMember, command.Notes ?? string.Empty));
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }
}
