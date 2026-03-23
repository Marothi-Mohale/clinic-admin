namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public sealed record DuplicatePatientMatch(
    Guid PatientId,
    int Score,
    DuplicateMatchStrength Strength,
    DuplicateActionRecommendation Recommendation,
    IReadOnlyCollection<DuplicateMatchReason> Reasons);

