using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Infrastructure.Auditing;
using ClinicAdmin.Infrastructure.Configuration;
using ClinicAdmin.Infrastructure.Persistence;
using ClinicAdmin.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ClinicAdmin.Infrastructure.Tests.Security;

public sealed class AuthenticationServiceTests
{
    private readonly Guid _facilityId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldAuthenticateAndCreateAuditEntry()
    {
        await using var dbContext = CreateDbContext();
        var passwordHasher = new Pbkdf2PasswordHasher();
        var passwordHash = passwordHasher.HashPassword("Admin@123");
        dbContext.Users.Add(new AppUser(_facilityId, "admin", "Administrator", passwordHash.Hash, passwordHash.Salt, UserRole.Admin));
        await dbContext.SaveChangesAsync();

        var sessionService = new UserSessionService();
        var authService = CreateAuthenticationService(dbContext, passwordHasher, sessionService);

        var result = await authService.LoginAsync("admin", "Admin@123");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Session);
        Assert.True(sessionService.IsAuthenticated);
        Assert.Single(dbContext.AuditEntries);
        Assert.True(dbContext.AuditEntries.Single().Succeeded);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldFailAndAuditAttempt()
    {
        await using var dbContext = CreateDbContext();
        var passwordHasher = new Pbkdf2PasswordHasher();
        var passwordHash = passwordHasher.HashPassword("Admin@123");
        dbContext.Users.Add(new AppUser(_facilityId, "admin", "Administrator", passwordHash.Hash, passwordHash.Salt, UserRole.Admin));
        await dbContext.SaveChangesAsync();

        var sessionService = new UserSessionService();
        var authService = CreateAuthenticationService(dbContext, passwordHasher, sessionService);

        var result = await authService.LoginAsync("admin", "WrongPassword");

        Assert.False(result.Succeeded);
        Assert.Equal(AuthenticationErrorCode.InvalidCredentials, result.ErrorCode);
        Assert.False(sessionService.IsAuthenticated);
        Assert.Single(dbContext.AuditEntries);
        Assert.False(dbContext.AuditEntries.Single().Succeeded);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldFail()
    {
        await using var dbContext = CreateDbContext();
        var passwordHasher = new Pbkdf2PasswordHasher();
        var passwordHash = passwordHasher.HashPassword("Admin@123");
        var user = new AppUser(_facilityId, "manager", "Manager", passwordHash.Hash, passwordHash.Salt, UserRole.Manager);
        user.SetActive(false);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var authService = CreateAuthenticationService(dbContext, passwordHasher, new UserSessionService());

        var result = await authService.LoginAsync("manager", "Admin@123");

        Assert.False(result.Succeeded);
        Assert.Equal(AuthenticationErrorCode.InvalidCredentials, result.ErrorCode);
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearSession()
    {
        await using var dbContext = CreateDbContext();
        var passwordHasher = new Pbkdf2PasswordHasher();
        var passwordHash = passwordHasher.HashPassword("Admin@123");
        dbContext.Users.Add(new AppUser(_facilityId, "admin", "Administrator", passwordHash.Hash, passwordHash.Salt, UserRole.Admin));
        await dbContext.SaveChangesAsync();

        var sessionService = new UserSessionService();
        var authService = CreateAuthenticationService(dbContext, passwordHasher, sessionService);

        await authService.LoginAsync("admin", "Admin@123");
        await authService.LogoutAsync();

        Assert.False(sessionService.IsAuthenticated);
        Assert.Null(sessionService.CurrentSession);
        Assert.Equal(2, dbContext.AuditEntries.Count());
        Assert.Contains(dbContext.AuditEntries, x => x.Action == "Logout");
    }

    [Fact]
    public async Task LoginAsync_WithBlankUsername_ShouldFailValidationAndAuditAttempt()
    {
        await using var dbContext = CreateDbContext();
        var authService = CreateAuthenticationService(dbContext, new Pbkdf2PasswordHasher(), new UserSessionService());

        var result = await authService.LoginAsync(string.Empty, "Admin@123");

        Assert.False(result.Succeeded);
        Assert.Equal(AuthenticationErrorCode.ValidationFailed, result.ErrorCode);
        Assert.Single(dbContext.AuditEntries);
        Assert.False(dbContext.AuditEntries.Single().Succeeded);
    }

    [Fact]
    public async Task LoginAsync_AfterRepeatedFailures_ShouldTemporarilyLockSignIn()
    {
        await using var dbContext = CreateDbContext();
        var passwordHasher = new Pbkdf2PasswordHasher();
        var passwordHash = passwordHasher.HashPassword("Admin@123");
        dbContext.Users.Add(new AppUser(_facilityId, "admin", "Administrator", passwordHash.Hash, passwordHash.Salt, UserRole.Admin));
        await dbContext.SaveChangesAsync();

        var sessionService = new UserSessionService();
        var limiter = CreateLimiter(maxFailedAttempts: 3, lockoutDurationMinutes: 10);
        var authService = CreateAuthenticationService(dbContext, passwordHasher, sessionService, limiter);

        await authService.LoginAsync("admin", "Wrong-1");
        await authService.LoginAsync("admin", "Wrong-2");
        var thirdFailure = await authService.LoginAsync("admin", "Wrong-3");
        var lockedOutAttempt = await authService.LoginAsync("admin", "Admin@123");

        Assert.False(thirdFailure.Succeeded);
        Assert.False(lockedOutAttempt.Succeeded);
        Assert.Equal(AuthenticationErrorCode.LockedOut, lockedOutAttempt.ErrorCode);
    }

    private ClinicAdminDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClinicAdminDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ClinicAdminDbContext(options);
    }

    private AuthenticationService CreateAuthenticationService(
        ClinicAdminDbContext dbContext,
        IPasswordHasher passwordHasher,
        IUserSessionService sessionService,
        ILoginAttemptLimiter? loginAttemptLimiter = null)
    {
        var facilityContext = new FakeFacilityContext(_facilityId);
        var auditService = new AuditService(
            dbContext,
            (ICurrentUserService)sessionService,
            new FakeClock(),
            facilityContext,
            new FakeWorkstationContext(),
            NullLogger<AuditService>.Instance);

        return new AuthenticationService(
            dbContext,
            passwordHasher,
            sessionService,
            auditService,
            new FakeClock(),
            facilityContext,
            loginAttemptLimiter ?? CreateLimiter(),
            new ValidatorExecutor<LoginRequest>(new[] { new LoginRequestValidator() }),
            NullLogger<AuthenticationService>.Instance);
    }

    private static ILoginAttemptLimiter CreateLimiter(int maxFailedAttempts = 5, int lockoutDurationMinutes = 5) =>
        new InMemoryLoginAttemptLimiter(Options.Create(new AuthenticationOptions
        {
            MaxFailedAttempts = maxFailedAttempts,
            LockoutDurationMinutes = lockoutDurationMinutes
        }));

    private sealed class FakeFacilityContext : IFacilityContext
    {
        public FakeFacilityContext(Guid facilityId)
        {
            CurrentFacilityId = facilityId;
        }

        public Guid CurrentFacilityId { get; }
        public string FacilityCode => "MAIN";
    }

    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 3, 23, 7, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeWorkstationContext : IWorkstationContext
    {
        public string WorkstationName => "TEST-WS";
    }
}
