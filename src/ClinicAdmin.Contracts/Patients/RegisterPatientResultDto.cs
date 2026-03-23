namespace ClinicAdmin.Contracts.Patients;

public sealed record RegisterPatientResultDto(
    bool Succeeded,
    bool RequiresConfirmation,
    string Message,
    Guid? PatientId,
    string? PatientNumber,
    IReadOnlyCollection<DuplicatePatientWarningDto> DuplicateWarnings);

