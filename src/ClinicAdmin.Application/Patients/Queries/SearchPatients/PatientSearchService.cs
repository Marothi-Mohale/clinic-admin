using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Contracts.Patients;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Patients.Queries.SearchPatients;

public sealed class PatientSearchService : IPatientSearchService
{
    private const int MaxTake = 50;
    private readonly IApplicationDbContext _dbContext;

    public PatientSearchService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<PatientSearchResultDto>> SearchAsync(SearchPatientsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            return Array.Empty<PatientSearchResultDto>();
        }

        var term = query.SearchTerm.Trim();
        var normalizedUpper = term.ToUpperInvariant();
        var digits = new string(term.Where(char.IsDigit).ToArray());
        var phoneSearchTerm = NormalizePhoneSearchTerm(digits);
        var namePattern = $"%{normalizedUpper}%";
        var take = Math.Clamp(query.Take, 1, MaxTake);

        var patients = _dbContext.Patients.AsNoTracking().Where(x => x.FacilityId == query.FacilityId);

        var results = await (
            from patient in patients
            join file in _dbContext.Files.AsNoTracking()
                on patient.Id equals file.PatientId into fileJoin
            from file in fileJoin.DefaultIfEmpty()
            let fullName = (patient.FirstName + " " + patient.LastName).ToUpper()
            let normalizedPhone = patient.PhoneNumber != null
                ? patient.PhoneNumber.Replace(" ", string.Empty)
                    .Replace("+", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace("(", string.Empty)
                    .Replace(")", string.Empty)
                : null
            where patient.PatientNumber.ToUpper().Contains(normalizedUpper) ||
                  (patient.NationalIdNumber != null && patient.NationalIdNumber.ToUpper().Contains(normalizedUpper)) ||
                  (patient.PassportNumber != null && patient.PassportNumber.ToUpper().Contains(normalizedUpper)) ||
                  (phoneSearchTerm != null && normalizedPhone != null && normalizedPhone.Contains(phoneSearchTerm)) ||
                  EF.Functions.Like(patient.LastName.ToUpper(), namePattern) ||
                  EF.Functions.Like(fullName, namePattern)
            select new
            {
                patient.Id,
                patient.PatientNumber,
                patient.FirstName,
                patient.LastName,
                patient.DateOfBirth,
                patient.NationalIdNumber,
                patient.PassportNumber,
                patient.PhoneNumber,
                FileNumber = file != null ? file.FileNumber : null,
                FileStatus = file != null ? file.Status.ToString() : null,
                FileLocation = file != null ? file.CurrentLocation : null,
                Score =
                    (patient.PatientNumber.ToUpper() == normalizedUpper ? 100 : 0) +
                    (patient.NationalIdNumber != null && patient.NationalIdNumber.ToUpper() == normalizedUpper ? 95 : 0) +
                    (patient.PassportNumber != null && patient.PassportNumber.ToUpper() == normalizedUpper ? 90 : 0) +
                    (phoneSearchTerm != null && normalizedPhone != null && normalizedPhone.Contains(phoneSearchTerm) ? 70 : 0) +
                    (patient.LastName.ToUpper() == normalizedUpper ? 60 : 0) +
                    (fullName == normalizedUpper ? 80 : 0)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Skip(query.Skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return results
            .Select(x => new PatientSearchResultDto(
                x.Id,
                x.PatientNumber,
                $"{x.FirstName} {x.LastName}",
                x.DateOfBirth,
                x.NationalIdNumber,
                x.PassportNumber,
                x.PhoneNumber,
                x.FileNumber,
                x.FileStatus,
                x.FileLocation))
            .ToArray();
    }

    public async Task<PatientProfileDto?> GetProfileAsync(Guid facilityId, Guid patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.Id == patientId)
            .Select(x => new
            {
                x.Id,
                x.PatientNumber,
                x.FirstName,
                x.LastName,
                x.DateOfBirth,
                Sex = x.Sex.ToString(),
                x.NationalIdNumber,
                x.PassportNumber,
                x.PhoneNumber,
                x.Address.Line1,
                x.Address.Line2,
                x.Address.Suburb,
                x.Address.City,
                NextOfKinName = x.NextOfKin.FullName,
                NextOfKinRelationship = x.NextOfKin.Relationship,
                NextOfKinPhoneNumber = x.NextOfKin.PhoneNumber
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (patient is null)
        {
            return null;
        }

        var file = await _dbContext.Files
            .AsNoTracking()
            .Where(x => x.PatientId == patientId && x.FacilityId == facilityId)
            .Select(x => new
            {
                x.FileNumber,
                FileStatus = x.Status.ToString(),
                x.CurrentLocation
            })
            .SingleOrDefaultAsync(cancellationToken);

        var history = await _dbContext.AuditEntries
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.EntityId == patientId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(8)
            .Select(x => new PatientHistoryItemDto(x.OccurredAtUtc, x.Action, x.Details, x.Succeeded))
            .ToListAsync(cancellationToken);

        var visitHistory = await _dbContext.Visits
            .AsNoTracking()
            .Where(x => x.FacilityId == facilityId && x.PatientId == patientId)
            .OrderByDescending(x => x.ArrivedAtUtc)
            .Take(5)
            .Select(x => new PatientHistoryItemDto(
                x.ArrivedAtUtc,
                "Visit",
                $"{x.ReasonForVisit} | {x.State} | {x.QueueStatus}",
                true))
            .ToListAsync(cancellationToken);

        var combinedHistory = history
            .Concat(visitHistory)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(8)
            .ToArray();

        return new PatientProfileDto(
            patient.Id,
            patient.PatientNumber,
            patient.FirstName,
            patient.LastName,
            patient.DateOfBirth,
            patient.Sex,
            patient.NationalIdNumber,
            patient.PassportNumber,
            patient.PhoneNumber,
            patient.Line1,
            patient.Line2,
            patient.Suburb,
            patient.City,
            patient.NextOfKinName,
            patient.NextOfKinRelationship,
            patient.NextOfKinPhoneNumber,
            file?.FileNumber,
            file?.FileStatus,
            file?.CurrentLocation,
            combinedHistory);
    }

    private static string? NormalizePhoneSearchTerm(string digits)
    {
        if (string.IsNullOrWhiteSpace(digits))
        {
            return null;
        }

        if (digits.Length == 11 && digits.StartsWith("27", StringComparison.Ordinal))
        {
            return $"0{digits[2..]}";
        }

        if (digits.Length == 9)
        {
            return $"0{digits}";
        }

        return digits;
    }
}
