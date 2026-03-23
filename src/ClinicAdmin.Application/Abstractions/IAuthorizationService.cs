using ClinicAdmin.Domain.Security;

namespace ClinicAdmin.Application.Abstractions;

public interface IAuthorizationService
{
    bool CanAccess(UserRole role, string feature);
    IReadOnlyCollection<string> GetAllowedFeatures(UserRole role);
}

