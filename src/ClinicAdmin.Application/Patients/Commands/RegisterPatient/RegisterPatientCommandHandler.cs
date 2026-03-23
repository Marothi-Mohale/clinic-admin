using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using ClinicAdmin.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed class RegisterPatientCommandHandler : IPatientRegistrationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ISyncJournal _syncJournal;
    private readonly IPatientRegistrationDuplicateWarningService _duplicateWarningService;
    private readonly ValidatorExecutor<RegisterPatientCommand> _validatorExecutor;

    public RegisterPatientCommandHandler(
        IApplicationDbContext dbContext,
        IAuditService auditService,
        ISyncJournal syncJournal,
        IPatientRegistrationDuplicateWarningService duplicateWarningService,
        ValidatorExecutor<RegisterPatientCommand> validatorExecutor)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _syncJournal = syncJournal;
        _duplicateWarningService = duplicateWarningService;
        _validatorExecutor = validatorExecutor;
    }

    public Task<RegisterPatientCommandResult> HandleAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default) =>
        RegisterAsync(command, cancellationToken);

    public async Task<RegisterPatientCommandResult> RegisterAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default)
    {
        await _validatorExecutor.ValidateAndThrowAsync(command, cancellationToken);

        var patientNumberExists = await _dbContext.Patients.AnyAsync(
            x => x.FacilityId == command.FacilityId && x.PatientNumber == command.PatientNumber,
            cancellationToken);

        if (patientNumberExists)
        {
            return RegisterPatientCommandResult.Failure("The patient number is already in use. Please capture a unique patient number.");
        }

        var duplicateWarnings = await _duplicateWarningService.CheckAsync(command, cancellationToken);
        var warnings = duplicateWarnings.Warnings;

        if (duplicateWarnings.Recommendation == DuplicateActionRecommendation.BlockCreation)
        {
            return RegisterPatientCommandResult.Failure("A patient with a matching government identifier already exists. Open the existing record instead of registering a new patient.", warnings);
        }

        if (duplicateWarnings.Recommendation == DuplicateActionRecommendation.RequireManualReview)
        {
            return RegisterPatientCommandResult.Failure("Possible duplicate patient found. Review the suggested matches before registration can continue.", warnings);
        }

        if (duplicateWarnings.Recommendation == DuplicateActionRecommendation.ShowWarning &&
            warnings.Count > 0 &&
            !command.DuplicateWarningAcknowledged)
        {
            return RegisterPatientCommandResult.ConfirmationRequired("Possible duplicate matches were found. Review the warning list and save again to confirm this is a new patient.", warnings);
        }

        var patient = new Patient(
            command.FacilityId,
            command.PatientNumber,
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.Sex,
            command.NationalIdNumber,
            command.PassportNumber,
            command.PhoneNumber,
            new Address(command.AddressLine1, command.AddressLine2, command.Suburb, command.City),
            new NextOfKin(command.NextOfKinName, command.NextOfKinRelationship, command.NextOfKinPhoneNumber));

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.WriteChangeAsync(
            "PatientRegistered",
            nameof(Patient),
            patient.Id,
            $"{patient.PatientNumber} {patient.FirstName} {patient.LastName}",
            afterSummary: BuildPatientSummary(patient),
            metadata: $"{{\"patientNumber\":\"{patient.PatientNumber}\",\"sex\":\"{patient.Sex}\",\"hasNationalId\":{(!string.IsNullOrWhiteSpace(patient.NationalIdNumber)).ToString().ToLowerInvariant()},\"hasPassport\":{(!string.IsNullOrWhiteSpace(patient.PassportNumber)).ToString().ToLowerInvariant()}}}",
            cancellationToken: cancellationToken);
        await _syncJournal.EnqueueAsync(
            "PatientRegistered",
            nameof(Patient),
            patient.Id,
            patient.FacilityId,
            $"{{\"patientId\":\"{patient.Id}\",\"patientNumber\":\"{patient.PatientNumber}\",\"facilityId\":\"{patient.FacilityId}\"}}",
            cancellationToken);

        return RegisterPatientCommandResult.Success(patient.Id, patient.PatientNumber);
    }

    private static string BuildPatientSummary(Patient patient)
    {
        var dateOfBirth = patient.DateOfBirth?.ToString("yyyy-MM-dd") ?? "unknown";
        return $"{patient.PatientNumber} | {patient.FirstName} {patient.LastName} | DOB: {dateOfBirth} | Sex: {patient.Sex}";
    }
}
