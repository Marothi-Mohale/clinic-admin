namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public sealed record DuplicatePatientCheckResult(
    DuplicateActionRecommendation Recommendation,
    IReadOnlyCollection<DuplicatePatientMatch> Matches)
{
    public bool HasMatches => Matches.Count > 0;
}

