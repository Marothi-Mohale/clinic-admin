namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public interface IPatientRegistrationDuplicateWarningService
{
    Task<DuplicateWarningResult> CheckAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default);
}

