using ClinicAdmin.Application.Patients.DuplicateDetection;
using ClinicAdmin.Contracts.Patients;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed class PatientRegistrationDuplicateWarningService : IPatientRegistrationDuplicateWarningService
{
    private readonly IPatientRegistrationDuplicateQueryService _duplicateQueryService;
    private readonly IPatientDuplicateDetectionService _duplicateDetectionService;

    public PatientRegistrationDuplicateWarningService(
        IPatientRegistrationDuplicateQueryService duplicateQueryService,
        IPatientDuplicateDetectionService duplicateDetectionService)
    {
        _duplicateQueryService = duplicateQueryService;
        _duplicateDetectionService = duplicateDetectionService;
    }

    public async Task<DuplicateWarningResult> CheckAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default)
    {
        var candidates = await _duplicateQueryService.FindCandidatesAsync(command, cancellationToken);
        var duplicateCheck = _duplicateDetectionService.Detect(
            new DuplicatePatientCheckRequest(
                command.FacilityId,
                command.FirstName,
                command.LastName,
                command.DateOfBirth,
                command.NationalIdNumber,
                command.PassportNumber,
                command.PhoneNumber),
            candidates);

        var warnings = candidates
            .Join(
                duplicateCheck.Matches,
                candidate => candidate.PatientId,
                match => match.PatientId,
                (candidate, match) => new DuplicatePatientWarningDto(
                    candidate.PatientId,
                    match.Score,
                    match.Strength.ToString(),
                    match.Recommendation.ToString(),
                    $"{candidate.FirstName} {candidate.LastName}",
                    candidate.DateOfBirth,
                    candidate.PatientNumber,
                    candidate.NationalIdNumber,
                    candidate.PassportNumber,
                    candidate.PhoneNumber,
                    match.Reasons.Select(reason => reason.ToString()).ToArray()))
            .OrderByDescending(x => x.Score)
            .ToArray();

        return new DuplicateWarningResult(duplicateCheck.Recommendation, warnings);
    }
}
