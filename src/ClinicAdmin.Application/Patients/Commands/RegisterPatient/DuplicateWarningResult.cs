using ClinicAdmin.Contracts.Patients;
using ClinicAdmin.Application.Patients.DuplicateDetection;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed record DuplicateWarningResult(
    DuplicateActionRecommendation Recommendation,
    IReadOnlyCollection<DuplicatePatientWarningDto> Warnings)
{
    public bool HasWarnings => Warnings.Count > 0;
}
