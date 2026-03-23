namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public interface IPatientRegistrationService
{
    Task<RegisterPatientCommandResult> RegisterAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default);
}

