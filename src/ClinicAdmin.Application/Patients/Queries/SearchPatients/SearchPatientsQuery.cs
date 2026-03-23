namespace ClinicAdmin.Application.Patients.Queries.SearchPatients;

public sealed record SearchPatientsQuery(
    Guid FacilityId,
    string? SearchTerm,
    int Skip = 0,
    int Take = 25);
