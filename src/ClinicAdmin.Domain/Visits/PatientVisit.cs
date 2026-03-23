using ClinicAdmin.Domain.Common;

namespace ClinicAdmin.Domain.Visits;

public sealed class PatientVisit : Entity
{
    private PatientVisit()
    {
        ReasonForVisit = string.Empty;
        Notes = string.Empty;
    }

    public Guid PatientId { get; private set; }
    public Guid FacilityId { get; private set; }
    public string ReasonForVisit { get; private set; }
    public QueueStatus QueueStatus { get; private set; }
    public VisitState State { get; private set; }
    public string? Department { get; private set; }
    public string? AssignedStaffMember { get; private set; }
    public string Notes { get; private set; }
    public DateTimeOffset ArrivedAtUtc { get; private set; }
    public DateTimeOffset LastUpdatedAtUtc { get; private set; }

    public PatientVisit(
        Guid patientId,
        Guid facilityId,
        string reasonForVisit,
        QueueStatus queueStatus,
        VisitState state,
        string? department,
        string? assignedStaffMember,
        string? notes,
        DateTimeOffset arrivedAtUtc)
    {
        PatientId = patientId == Guid.Empty ? throw new ArgumentException("Patient is required.", nameof(patientId)) : patientId;
        FacilityId = facilityId == Guid.Empty ? throw new ArgumentException("Facility is required.", nameof(facilityId)) : facilityId;
        ReasonForVisit = GuardRequired(reasonForVisit, nameof(reasonForVisit));
        QueueStatus = queueStatus;
        State = state;
        Department = Normalize(department);
        AssignedStaffMember = Normalize(assignedStaffMember);
        Notes = Normalize(notes) ?? string.Empty;
        ArrivedAtUtc = arrivedAtUtc;
        LastUpdatedAtUtc = arrivedAtUtc;
    }

    public void UpdateWorkflow(
        QueueStatus queueStatus,
        VisitState state,
        string? department,
        string? assignedStaffMember,
        string? notes,
        DateTimeOffset updatedAtUtc)
    {
        QueueStatus = queueStatus;
        State = state;
        Department = Normalize(department);
        AssignedStaffMember = Normalize(assignedStaffMember);
        Notes = Normalize(notes) ?? string.Empty;
        LastUpdatedAtUtc = updatedAtUtc;
    }

    private static string GuardRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.", fieldName);
        }

        return value.Trim();
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

