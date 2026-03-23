using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Security;

namespace ClinicAdmin.Application.Authorization;

public sealed class AuthorizationService : IAuthorizationService
{
    private static readonly IReadOnlyDictionary<UserRole, string[]> FeatureMap = new Dictionary<UserRole, string[]>
    {
        [UserRole.Admin] = ["Dashboard", "Patients", "Register", "Visits", "Reports", "Audit", "Administration"],
        [UserRole.Receptionist] = ["Dashboard", "Patients", "Register", "Visits", "Files"],
        [UserRole.Nurse] = ["Dashboard", "Patients", "Visits"],
        [UserRole.Doctor] = ["Dashboard", "Patients", "History"],
        [UserRole.Manager] = ["Dashboard", "Reports", "Audit"]
    };

    public bool CanAccess(UserRole role, string feature) =>
        FeatureMap.TryGetValue(role, out var features) &&
        features.Contains(feature, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> GetAllowedFeatures(UserRole role) =>
        FeatureMap.TryGetValue(role, out var features)
            ? features
            : Array.Empty<string>();
}
