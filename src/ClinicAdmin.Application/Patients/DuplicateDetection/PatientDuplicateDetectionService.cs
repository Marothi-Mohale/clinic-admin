using System.Globalization;
using System.Text;

namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public sealed class PatientDuplicateDetectionService : IPatientDuplicateDetectionService
{
    public DuplicatePatientCheckResult Detect(
        DuplicatePatientCheckRequest request,
        IEnumerable<DuplicatePatientCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(candidates);

        var normalizedRequest = NormalizedPatientIdentity.FromRequest(request);

        var matches = candidates
            .Where(candidate => candidate.FacilityId == request.FacilityId && candidate.PatientId != request.ExistingPatientId)
            .Select(candidate => ScoreCandidate(normalizedRequest, candidate))
            .Where(match => match is not null)
            .Cast<DuplicatePatientMatch>()
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.PatientId)
            .Take(10)
            .ToArray();

        var recommendation = matches.Length == 0
            ? DuplicateActionRecommendation.SafeToCreate
            : matches.Max(match => match.Recommendation);

        return new DuplicatePatientCheckResult(recommendation, matches);
    }

    private static DuplicatePatientMatch? ScoreCandidate(NormalizedPatientIdentity request, DuplicatePatientCandidate candidate)
    {
        var normalizedCandidate = NormalizedPatientIdentity.FromCandidate(candidate);
        var reasons = new HashSet<DuplicateMatchReason>();

        if (request.NormalizedNationalId is not null &&
            request.NormalizedNationalId == normalizedCandidate.NormalizedNationalId)
        {
            reasons.Add(DuplicateMatchReason.ExactNationalId);
            return BuildMatch(candidate.PatientId, 100, reasons);
        }

        if (request.NormalizedPassportNumber is not null &&
            request.NormalizedPassportNumber == normalizedCandidate.NormalizedPassportNumber)
        {
            reasons.Add(DuplicateMatchReason.ExactPassportNumber);
            return BuildMatch(candidate.PatientId, 95, reasons);
        }

        var score = 0;
        var exactFirstName = request.NormalizedFirstName is not null &&
                             request.NormalizedFirstName == normalizedCandidate.NormalizedFirstName;
        var exactLastName = request.NormalizedLastName is not null &&
                            request.NormalizedLastName == normalizedCandidate.NormalizedLastName;
        var exactFullName = request.NormalizedFullName is not null &&
                            request.NormalizedFullName == normalizedCandidate.NormalizedFullName;

        if (request.NormalizedPhoneNumber is not null &&
            request.NormalizedPhoneNumber == normalizedCandidate.NormalizedPhoneNumber)
        {
            score += 25;
            reasons.Add(DuplicateMatchReason.ExactPhoneNumber);
        }

        var sameDateOfBirth = request.DateOfBirth is not null &&
                              request.DateOfBirth == normalizedCandidate.DateOfBirth;
        if (sameDateOfBirth)
        {
            score += 10;
            reasons.Add(DuplicateMatchReason.MatchingDateOfBirth);
        }

        if (exactFullName)
        {
            score += 35;
            reasons.Add(DuplicateMatchReason.ExactFullName);
        }

        if (exactLastName && sameDateOfBirth)
        {
            score += 35;
            reasons.Add(DuplicateMatchReason.ExactSurnameAndDateOfBirth);
        }

        var firstNameSimilarity = Similarity(request.NormalizedFirstName, normalizedCandidate.NormalizedFirstName);
        var surnameSimilarity = Similarity(request.NormalizedLastName, normalizedCandidate.NormalizedLastName);
        var fullNameSimilarity = Similarity(request.NormalizedFullName, normalizedCandidate.NormalizedFullName);

        if (!exactFirstName && firstNameSimilarity >= 0.90m)
        {
            score += 10;
            reasons.Add(DuplicateMatchReason.SimilarFirstName);
        }

        if (!exactLastName && surnameSimilarity >= 0.92m)
        {
            score += 15;
            reasons.Add(DuplicateMatchReason.SimilarSurname);
        }

        if (!exactFullName && fullNameSimilarity >= 0.90m)
        {
            score += 20;
            reasons.Add(DuplicateMatchReason.SimilarFullName);
        }

        if (request.FirstInitial is not null &&
            request.FirstInitial == normalizedCandidate.FirstInitial &&
            exactLastName &&
            !exactFirstName)
        {
            score += 12;
            reasons.Add(DuplicateMatchReason.MatchingInitialAndSurname);
        }

        if (score < 30 || reasons.Count == 0)
        {
            return null;
        }

        return BuildMatch(candidate.PatientId, score, reasons);
    }

    private static DuplicatePatientMatch BuildMatch(Guid patientId, int score, IReadOnlyCollection<DuplicateMatchReason> reasons)
    {
        var strength = score switch
        {
            >= 95 => DuplicateMatchStrength.Critical,
            >= 65 => DuplicateMatchStrength.High,
            >= 45 => DuplicateMatchStrength.Medium,
            _ => DuplicateMatchStrength.Low
        };

        var recommendation = score switch
        {
            >= 95 => DuplicateActionRecommendation.BlockCreation,
            >= 65 => DuplicateActionRecommendation.RequireManualReview,
            _ => DuplicateActionRecommendation.ShowWarning
        };

        return new DuplicatePatientMatch(patientId, score, strength, recommendation, reasons);
    }

    private static decimal Similarity(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return 0m;
        }

        if (left == right)
        {
            return 1m;
        }

        var distance = LevenshteinDistance(left, right);
        var maxLength = Math.Max(left.Length, right.Length);

        return maxLength == 0
            ? 1m
            : Math.Round(1m - (decimal)distance / maxLength, 4, MidpointRounding.AwayFromZero);
    }

    private static int LevenshteinDistance(string left, string right)
    {
        var costs = new int[right.Length + 1];

        for (var j = 0; j <= right.Length; j++)
        {
            costs[j] = j;
        }

        for (var i = 1; i <= left.Length; i++)
        {
            var previousDiagonal = costs[0];
            costs[0] = i;

            for (var j = 1; j <= right.Length; j++)
            {
                var temp = costs[j];
                var substitutionCost = left[i - 1] == right[j - 1] ? 0 : 1;

                costs[j] = Math.Min(
                    Math.Min(costs[j] + 1, costs[j - 1] + 1),
                    previousDiagonal + substitutionCost);

                previousDiagonal = temp;
            }
        }

        return costs[right.Length];
    }

    private sealed record NormalizedPatientIdentity(
        DateOnly? DateOfBirth,
        string? NormalizedFirstName,
        string? NormalizedLastName,
        string? NormalizedFullName,
        string? NormalizedNationalId,
        string? NormalizedPassportNumber,
        string? NormalizedPhoneNumber,
        string? FirstInitial)
    {
        public static NormalizedPatientIdentity FromRequest(DuplicatePatientCheckRequest request) =>
            new(
                request.DateOfBirth,
                NormalizeName(request.FirstName),
                NormalizeName(request.LastName),
                NormalizeName($"{request.FirstName} {request.LastName}"),
                NormalizeIdentifier(request.NationalIdNumber),
                NormalizeIdentifier(request.PassportNumber),
                NormalizePhone(request.PhoneNumber),
                NormalizeInitial(request.FirstName));

        public static NormalizedPatientIdentity FromCandidate(DuplicatePatientCandidate candidate) =>
            new(
                candidate.DateOfBirth,
                NormalizeName(candidate.FirstName),
                NormalizeName(candidate.LastName),
                NormalizeName($"{candidate.FirstName} {candidate.LastName}"),
                NormalizeIdentifier(candidate.NationalIdNumber),
                NormalizeIdentifier(candidate.PassportNumber),
                NormalizePhone(candidate.PhoneNumber),
                NormalizeInitial(candidate.FirstName));

        private static string? NormalizeInitial(string? value)
        {
            var normalized = NormalizeName(value);
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized[..1];
        }

        private static string? NormalizeName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
                else if (char.IsWhiteSpace(character) && builder.Length > 0 && builder[^1] != ' ')
                {
                    builder.Append(' ');
                }
            }

            return builder.ToString().Trim();
        }

        private static string? NormalizeIdentifier(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

        private static string? NormalizePhone(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length == 11 && digits.StartsWith("27", StringComparison.Ordinal))
            {
                return $"0{digits[2..]}";
            }

            if (digits.Length == 9)
            {
                return $"0{digits}";
            }

            return digits.Length == 0 ? null : digits;
        }
    }
}
