using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Contracts.Patients;
using ClinicAdmin.Desktop.ViewModels;
using ClinicAdmin.Domain.Patients;

namespace ClinicAdmin.Desktop.Tests.ViewModels;

public sealed class PatientRegistrationViewModelTests
{
    [Fact]
    public async Task CheckDuplicatesAsync_WhenWarningsExist_ShouldPopulateWarningPanel()
    {
        var viewModel = new PatientRegistrationViewModel(
            new FakePatientRegistrationService(),
            new FakeDuplicateWarningService(),
            new FakeFacilityContext())
        {
            PatientNumber = "P-700",
            FirstName = "Ayanda",
            LastName = "Zulu",
            DateOfBirth = new DateTime(1990, 1, 1),
            SelectedSex = Sex.Female,
            NationalIdNumber = "9001011234088"
        };

        viewModel.CheckDuplicatesCommand.Execute(null);
        await Task.Delay(10);

        Assert.True(viewModel.HasDuplicateWarnings);
        Assert.Single(viewModel.DuplicateWarnings);
    }

    private sealed class FakePatientRegistrationService : IPatientRegistrationService
    {
        public Task<RegisterPatientCommandResult> RegisterAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default) =>
            Task.FromResult(RegisterPatientCommandResult.Success(Guid.NewGuid(), command.PatientNumber));
    }

    private sealed class FakeDuplicateWarningService : IPatientRegistrationDuplicateWarningService
    {
        public Task<DuplicateWarningResult> CheckAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DuplicateWarningResult(
                ClinicAdmin.Application.Patients.DuplicateDetection.DuplicateActionRecommendation.ShowWarning,
                new[]
                {
                    new DuplicatePatientWarningDto(
                        Guid.NewGuid(),
                        60,
                        "High",
                        "ShowWarning",
                        "Ayanda Zulu",
                        new DateOnly(1990, 1, 1),
                        "P-001",
                        "9001011234088",
                        null,
                        "0821234567",
                        new[] { "ExactFullName", "MatchingDateOfBirth" })
                }));
        }
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }
}
