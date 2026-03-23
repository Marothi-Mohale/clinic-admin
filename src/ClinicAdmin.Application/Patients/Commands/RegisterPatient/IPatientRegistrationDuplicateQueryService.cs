using ClinicAdmin.Application.Patients.DuplicateDetection;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public interface IPatientRegistrationDuplicateQueryService
{
    Task<IReadOnlyCollection<DuplicatePatientCandidate>> FindCandidatesAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default);
}

