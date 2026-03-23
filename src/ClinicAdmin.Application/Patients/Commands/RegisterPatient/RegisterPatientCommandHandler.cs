using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Domain.Patients;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed class RegisterPatientCommandHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ISyncJournal _syncJournal;
    private readonly ValidatorExecutor<RegisterPatientCommand> _validatorExecutor;

    public RegisterPatientCommandHandler(
        IApplicationDbContext dbContext,
        IAuditService auditService,
        ISyncJournal syncJournal,
        ValidatorExecutor<RegisterPatientCommand> validatorExecutor)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _syncJournal = syncJournal;
        _validatorExecutor = validatorExecutor;
    }

    public async Task<Guid> HandleAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default)
    {
        await _validatorExecutor.ValidateAndThrowAsync(command, cancellationToken);

        var patient = new Patient(
            command.FacilityId,
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.NationalIdNumber,
            command.PhoneNumber);

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.WriteAsync("PatientRegistered", nameof(Patient), patient.Id, $"{patient.FirstName} {patient.LastName}", cancellationToken);
        await _syncJournal.EnqueueAsync(
            "PatientRegistered",
            nameof(Patient),
            patient.Id,
            patient.FacilityId,
            $"{{\"patientId\":\"{patient.Id}\",\"facilityId\":\"{patient.FacilityId}\"}}",
            cancellationToken);

        return patient.Id;
    }
}
