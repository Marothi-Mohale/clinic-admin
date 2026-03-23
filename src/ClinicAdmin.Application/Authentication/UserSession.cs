using ClinicAdmin.Domain.Security;

namespace ClinicAdmin.Application.Authentication;

public sealed record UserSession(
    Guid UserId,
    string Username,
    string DisplayName,
    UserRole Role,
    Guid FacilityId,
    DateTimeOffset LoggedInAtUtc);

