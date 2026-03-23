using ClinicAdmin.Application.Authorization;
using ClinicAdmin.Domain.Security;

namespace ClinicAdmin.Application.Tests.Authorization;

public sealed class AuthorizationServiceTests
{
    [Fact]
    public void CanAccess_WhenReceptionistRequestsAdministration_ShouldReturnFalse()
    {
        var service = new AuthorizationService();

        var result = service.CanAccess(UserRole.Receptionist, "Administration");

        Assert.False(result);
    }

    [Fact]
    public void GetAllowedFeatures_WhenManagerRole_ShouldContainAuditAndReports()
    {
        var service = new AuthorizationService();

        var features = service.GetAllowedFeatures(UserRole.Manager);

        Assert.Contains("Reports", features);
        Assert.Contains("Audit", features);
    }

    [Fact]
    public void GetAllowedFeatures_WhenAdminRole_ShouldContainAudit()
    {
        var service = new AuthorizationService();

        var features = service.GetAllowedFeatures(UserRole.Admin);

        Assert.Contains("Audit", features);
    }
}
