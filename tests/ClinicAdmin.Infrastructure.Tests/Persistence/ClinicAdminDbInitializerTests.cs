using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Infrastructure.Configuration;
using ClinicAdmin.Infrastructure.Persistence;
using ClinicAdmin.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ClinicAdmin.Infrastructure.Tests.Persistence;

public sealed class ClinicAdminDbInitializerTests
{
    [Fact]
    public async Task InitializeAsync_WhenDefaultUserSeedingDisabled_ShouldNotSeedUsers()
    {
        await using var dbContext = CreateDbContext();
        var initializer = CreateInitializer(dbContext, seedDefaultUsers: false);

        await initializer.InitializeAsync();

        Assert.Empty(dbContext.Users);
    }

    [Fact]
    public async Task InitializeAsync_WhenDefaultUserSeedingEnabled_ShouldSeedUsers()
    {
        await using var dbContext = CreateDbContext();
        var initializer = CreateInitializer(dbContext, seedDefaultUsers: true);

        await initializer.InitializeAsync();

        Assert.Equal(5, dbContext.Users.Count());
        Assert.Contains(dbContext.Users, x => x.Role == UserRole.Admin);
    }

    private static ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }

    private static ClinicAdminDbInitializer CreateInitializer(ClinicAdminDbContext dbContext, bool seedDefaultUsers)
    {
        return new ClinicAdminDbInitializer(
            dbContext,
            new Pbkdf2PasswordHasher(),
            new FakeFacilityContext(),
            Options.Create(new SeedingOptions { SeedDefaultUsers = seedDefaultUsers }),
            NullLogger<ClinicAdminDbInitializer>.Instance);
    }

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public Guid CurrentFacilityId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string FacilityCode => "MAIN";
    }
}
