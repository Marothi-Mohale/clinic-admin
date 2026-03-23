using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Patients.Commands.RegisterPatient;

public sealed class PatientRegistrationDuplicateQueryService : IPatientRegistrationDuplicateQueryService
{
    private readonly IApplicationDbContext _dbContext;

    public PatientRegistrationDuplicateQueryService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<DuplicatePatientCandidate>> FindCandidatesAsync(RegisterPatientCommand command, CancellationToken cancellationToken = default)
    {
        var phoneCandidates = BuildPhoneCandidates(command.PhoneNumber);
        var normalizedNationalId = NormalizeIdentifier(command.NationalIdNumber);
        var normalizedPassport = NormalizeIdentifier(command.PassportNumber);
        var normalizedLastName = NormalizeName(command.LastName);
        var normalizedFirstName = NormalizeName(command.FirstName);

        return await _dbContext.Patients
            .AsNoTracking()
            .Where(x => x.FacilityId == command.FacilityId)
            .Where(x =>
                x.PatientNumber == command.PatientNumber ||
                (normalizedNationalId != null && x.NationalIdNumber != null && x.NationalIdNumber.ToUpper() == normalizedNationalId) ||
                (normalizedPassport != null && x.PassportNumber != null && x.PassportNumber.ToUpper() == normalizedPassport) ||
                (phoneCandidates.Length > 0 && x.PhoneNumber != null && phoneCandidates.Contains(x.PhoneNumber)) ||
                (x.LastName.ToUpper() == normalizedLastName && x.DateOfBirth == command.DateOfBirth) ||
                (x.LastName.ToUpper() == normalizedLastName && x.FirstName.ToUpper() == normalizedFirstName))
            .Select(x => new DuplicatePatientCandidate(
                x.Id,
                x.FacilityId,
                x.PatientNumber,
                x.FirstName,
                x.LastName,
                x.DateOfBirth,
                x.NationalIdNumber,
                x.PassportNumber,
                x.PhoneNumber))
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeIdentifier(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : new string(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    private static string? NormalizeName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string[] BuildPhoneCandidates(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        var candidates = new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(value))
        {
            candidates.Add(value.Trim());
        }

        if (digits.Length > 0)
        {
            candidates.Add(digits);
        }

        if (digits.Length == 11 && digits.StartsWith("27", StringComparison.Ordinal))
        {
            candidates.Add($"0{digits[2..]}");
        }
        else if (digits.Length == 9)
        {
            candidates.Add($"0{digits}");
        }
        return candidates.ToArray();
    }
}
