using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicAdmin.Infrastructure.Persistence;

public sealed class ClinicAdminDbInitializer
{
    private readonly ClinicAdminDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IFacilityContext _facilityContext;
    private readonly ILogger<ClinicAdminDbInitializer> _logger;

    public ClinicAdminDbInitializer(
        ClinicAdminDbContext dbContext,
        IPasswordHasher passwordHasher,
        IFacilityContext facilityContext,
        ILogger<ClinicAdminDbInitializer> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _facilityContext = facilityContext;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await _dbContext.Users.AnyAsync(x => x.FacilityId == _facilityContext.CurrentFacilityId, cancellationToken))
        {
            return;
        }

        var seedUsers = new[]
        {
            CreateUser("admin", "System Administrator", "Admin@123", UserRole.Admin),
            CreateUser("reception", "Reception Desk", "Reception@123", UserRole.Receptionist),
            CreateUser("nurse", "Duty Nurse", "Nurse@123", UserRole.Nurse),
            CreateUser("doctor", "Clinic Doctor", "Doctor@123", UserRole.Doctor),
            CreateUser("manager", "Clinic Manager", "Manager@123", UserRole.Manager)
        };

        _dbContext.Users.AddRange(seedUsers);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} default clinic users for facility {FacilityId}", seedUsers.Length, _facilityContext.CurrentFacilityId);
    }

    private AppUser CreateUser(string username, string displayName, string password, UserRole role)
    {
        var passwordHash = _passwordHasher.HashPassword(password);
        return new AppUser(
            _facilityContext.CurrentFacilityId,
            username,
            displayName,
            passwordHash.Hash,
            passwordHash.Salt,
            role);
    }
}
