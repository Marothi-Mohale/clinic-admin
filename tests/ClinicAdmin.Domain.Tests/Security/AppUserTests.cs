using ClinicAdmin.Domain.Security;

namespace ClinicAdmin.Domain.Tests.Security;

public sealed class AppUserTests
{
    [Fact]
    public void Constructor_ShouldNormalizeUsernameToUpperCase()
    {
        var user = new AppUser(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "reception",
            "Reception Desk",
            "hash",
            "salt",
            UserRole.Receptionist);

        Assert.Equal("RECEPTION", user.Username);
    }
}
