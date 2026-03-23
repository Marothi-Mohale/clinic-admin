namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public interface IPatientDuplicateDetectionService
{
    DuplicatePatientCheckResult Detect(
        DuplicatePatientCheckRequest request,
        IEnumerable<DuplicatePatientCandidate> candidates);
}

