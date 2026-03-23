using ClinicAdmin.Contracts.Patients;

namespace ClinicAdmin.Application.Patients.Queries.SearchPatients;

public interface IPatientSearchService
{
    Task<IReadOnlyCollection<PatientSearchResultDto>> SearchAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default);
    Task<PatientProfileDto?> GetProfileAsync(Guid facilityId, Guid patientId, CancellationToken cancellationToken = default);
}

