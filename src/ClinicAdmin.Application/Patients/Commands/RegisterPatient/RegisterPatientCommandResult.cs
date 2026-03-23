using ClinicAdmin.Contracts.Patients;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed record RegisterPatientCommandResult(
    bool Succeeded,
    bool RequiresConfirmation,
    string Message,
    Guid? PatientId,
    string? PatientNumber,
    IReadOnlyCollection<DuplicatePatientWarningDto> DuplicateWarnings)
{
    public static RegisterPatientCommandResult Success(Guid patientId, string patientNumber) =>
        new(true, false, "Patient registered successfully.", patientId, patientNumber, Array.Empty<DuplicatePatientWarningDto>());

    public static RegisterPatientCommandResult Failure(string message, IReadOnlyCollection<DuplicatePatientWarningDto>? warnings = null) =>
        new(false, false, message, null, null, warnings ?? Array.Empty<DuplicatePatientWarningDto>());

    public static RegisterPatientCommandResult ConfirmationRequired(string message, IReadOnlyCollection<DuplicatePatientWarningDto> warnings) =>
        new(false, true, message, null, null, warnings);
}

