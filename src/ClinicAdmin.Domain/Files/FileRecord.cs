using ClinicAdmin.Domain.Common;

namespace ClinicAdmin.Domain.Files;

public sealed class FileRecord : Entity
{
    public Guid PatientId { get; private set; }
    public Guid FacilityId { get; private set; }
    public string FileNumber { get; private set; }
    public FileStatus Status { get; private set; }
    public string? CurrentLocation { get; private set; }

    public FileRecord(Guid patientId, Guid facilityId, string fileNumber)
    {
        PatientId = patientId;
        FacilityId = facilityId;
        FileNumber = string.IsNullOrWhiteSpace(fileNumber)
            ? throw new ArgumentException("File number is required.", nameof(fileNumber))
            : fileNumber.Trim();
        Status = FileStatus.Available;
    }

    public void UpdateStatus(FileStatus status, string? currentLocation)
    {
        Status = status;
        CurrentLocation = string.IsNullOrWhiteSpace(currentLocation) ? null : currentLocation.Trim();
    }
}
