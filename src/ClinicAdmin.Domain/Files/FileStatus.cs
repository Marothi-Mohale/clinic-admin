namespace ClinicAdmin.Domain.Files;

public enum FileStatus
{
    Available = 1,
    Requested = 2,
    Issued = 3,
    InConsultation = 4,
    Returned = 5,
    Missing = 6
}

