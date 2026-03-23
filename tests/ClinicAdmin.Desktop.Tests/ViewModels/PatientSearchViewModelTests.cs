using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Contracts.Patients;
using ClinicAdmin.Desktop.ViewModels;

namespace ClinicAdmin.Desktop.Tests.ViewModels;

public sealed class PatientSearchViewModelTests
{
    [Fact]
    public async Task SearchCommand_WhenResultsExist_ShouldPopulateResultsAndProfile()
    {
        var viewModel = new PatientSearchViewModel(new FakePatientSearchService(), new FakeFacilityContext())
        {
            SearchTerm = "P-100"
        };

        viewModel.SearchCommand.Execute(null);
        await Task.Delay(20);

        Assert.True(viewModel.HasResults);
        Assert.NotNull(viewModel.SelectedResult);
        Assert.Equal("P-100", viewModel.SelectedProfile.PatientNumber);
    }

    private sealed class FakePatientSearchService : IPatientSearchService
    {
        public Task<IReadOnlyCollection<PatientSearchResultDto>> SearchAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<PatientSearchResultDto>>(new[]
            {
                new PatientSearchResultDto(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    "P-100",
                    "Nomsa Dlamini",
                    new DateOnly(1990, 1, 1),
                    "9001011234088",
                    null,
                    "0821234567",
                    "F-12",
                    "Available",
                    "Records room")
            });

        public Task<PatientProfileDto?> GetProfileAsync(Guid facilityId, Guid patientId, CancellationToken cancellationToken = default) =>
            Task.FromResult<PatientProfileDto?>(new PatientProfileDto(
                patientId,
                "P-100",
                "Nomsa",
                "Dlamini",
                new DateOnly(1990, 1, 1),
                "Female",
                "9001011234088",
                null,
                "0821234567",
                "12 Main Street",
                null,
                "Mamelodi",
                "Pretoria",
                "Sarah Dlamini",
                "Sister",
                "0827654321",
                "F-12",
                "Available",
                "Records room",
                new[]
                {
                    new PatientHistoryItemDto(DateTimeOffset.UtcNow, "PatientRegistered", "Patient registered", true)
                }));
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }
}
