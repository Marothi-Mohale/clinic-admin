using ClinicAdmin.Application.Patients.DuplicateDetection;

namespace ClinicAdmin.Application.Tests.Patients.DuplicateDetection;

public sealed class PatientDuplicateDetectionServiceTests
{
    private readonly PatientDuplicateDetectionService _service = new();
    private readonly Guid _facilityId = Guid.NewGuid();

    [Fact]
    public void Detect_WhenNationalIdMatchesExactly_ShouldBlockCreation()
    {
        var request = new DuplicatePatientCheckRequest(
            _facilityId,
            "Nomsa",
            "Dlamini",
            new DateOnly(1988, 4, 2),
            "8804021234088",
            null,
            "0821234567");

        var candidates = new[]
        {
            new DuplicatePatientCandidate(Guid.NewGuid(), _facilityId, "P-100", "Nomsa", "Dlamini", new DateOnly(1988, 4, 2), "8804021234088", null, "0821234567")
        };

        var result = _service.Detect(request, candidates);

        Assert.Equal(DuplicateActionRecommendation.BlockCreation, result.Recommendation);
        Assert.Single(result.Matches);
        Assert.Contains(DuplicateMatchReason.ExactNationalId, result.Matches[0].Reasons);
    }

    [Fact]
    public void Detect_WhenPassportMatchesExactly_ShouldBlockCreation()
    {
        var request = new DuplicatePatientCheckRequest(
            _facilityId,
            "Amina",
            "Khan",
            new DateOnly(1993, 10, 12),
            null,
            "P1234567",
            null);

        var candidates = new[]
        {
            new DuplicatePatientCandidate(Guid.NewGuid(), _facilityId, "P-101", "Ameena", "Khan", new DateOnly(1993, 10, 12), null, "P1234567", null)
        };

        var result = _service.Detect(request, candidates);

        Assert.Equal(DuplicateActionRecommendation.BlockCreation, result.Recommendation);
        Assert.Contains(DuplicateMatchReason.ExactPassportNumber, result.Matches[0].Reasons);
    }

    [Fact]
    public void Detect_WhenPhoneIsEquivalentInInternationalFormat_ShouldWarn()
    {
        var request = new DuplicatePatientCheckRequest(
            _facilityId,
            "Sipho",
            "Nkosi",
            new DateOnly(1997, 7, 9),
            null,
            null,
            "+27 82 123 4567");

        var candidates = new[]
        {
            new DuplicatePatientCandidate(Guid.NewGuid(), _facilityId, "P-102", "Sipho", "Nkosi", new DateOnly(1997, 7, 9), null, null, "0821234567")
        };

        var result = _service.Detect(request, candidates);

        Assert.Equal(DuplicateActionRecommendation.RequireManualReview, result.Recommendation);
        Assert.Contains(DuplicateMatchReason.ExactPhoneNumber, result.Matches[0].Reasons);
        Assert.Contains(DuplicateMatchReason.ExactSurnameAndDateOfBirth, result.Matches[0].Reasons);
    }

    [Fact]
    public void Detect_WhenSurnameDobAndCloseFirstNameMatch_ShouldRequireManualReview()
    {
        var request = new DuplicatePatientCheckRequest(
            _facilityId,
            "Nokuthula",
            "Maseko",
            new DateOnly(1985, 1, 5),
            null,
            null,
            null);

        var candidates = new[]
        {
            new DuplicatePatientCandidate(Guid.NewGuid(), _facilityId, "P-103", "Nokutula", "Maseko", new DateOnly(1985, 1, 5), null, null, null)
        };

        var result = _service.Detect(request, candidates);

        Assert.Equal(DuplicateActionRecommendation.RequireManualReview, result.Recommendation);
        Assert.Contains(DuplicateMatchReason.ExactSurnameAndDateOfBirth, result.Matches[0].Reasons);
        Assert.Contains(DuplicateMatchReason.SimilarFirstName, result.Matches[0].Reasons);
    }

    [Fact]
    public void Detect_WhenOnlyGenericSurnameMatches_ShouldAllowCreation()
    {
        var request = new DuplicatePatientCheckRequest(
            _facilityId,
            "Thabo",
            "Mokoena",
            new DateOnly(2000, 6, 15),
            null,
            null,
            null);

        var candidates = new[]
        {
            new DuplicatePatientCandidate(Guid.NewGuid(), _facilityId, "P-104", "Peter", "Mokoena", new DateOnly(1975, 2, 1), null, null, null)
        };

        var result = _service.Detect(request, candidates);

        Assert.Equal(DuplicateActionRecommendation.SafeToCreate, result.Recommendation);
        Assert.Empty(result.Matches);
    }

    [Fact]
    public void Detect_WhenOnlyFullNameMatchesExactly_ShouldShowWarning()
    {
        var request = new DuplicatePatientCheckRequest(
            _facilityId,
            "Ayanda",
            "Zulu",
            null,
            null,
            null,
            null);

        var candidates = new[]
        {
            new DuplicatePatientCandidate(Guid.NewGuid(), _facilityId, "P-105", "Ayanda", "Zulu", null, null, null, null)
        };

        var result = _service.Detect(request, candidates);

        Assert.Equal(DuplicateActionRecommendation.ShowWarning, result.Recommendation);
        Assert.Single(result.Matches);
        Assert.Contains(DuplicateMatchReason.ExactFullName, result.Matches[0].Reasons);
    }

    [Fact]
    public void Detect_WhenCandidateBelongsToDifferentFacility_ShouldIgnoreCandidate()
    {
        var request = new DuplicatePatientCheckRequest(
            _facilityId,
            "Lerato",
            "Molefe",
            new DateOnly(1994, 12, 11),
            "9412111234085",
            null,
            null);

        var candidates = new[]
        {
            new DuplicatePatientCandidate(Guid.NewGuid(), Guid.NewGuid(), "P-106", "Lerato", "Molefe", new DateOnly(1994, 12, 11), "9412111234085", null, null)
        };

        var result = _service.Detect(request, candidates);

        Assert.Equal(DuplicateActionRecommendation.SafeToCreate, result.Recommendation);
        Assert.Empty(result.Matches);
    }
}
