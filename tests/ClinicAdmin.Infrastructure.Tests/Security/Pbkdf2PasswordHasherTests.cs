using ClinicAdmin.Infrastructure.Security;

namespace ClinicAdmin.Infrastructure.Tests.Security;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void HashPassword_ShouldCreateVerifiableHash()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var result = hasher.HashPassword("Admin@123");

        Assert.NotEmpty(result.Hash);
        Assert.NotEmpty(result.Salt);
        Assert.True(hasher.VerifyPassword("Admin@123", result.Hash, result.Salt));
        Assert.False(hasher.VerifyPassword("WrongPassword", result.Hash, result.Salt));
    }
}

