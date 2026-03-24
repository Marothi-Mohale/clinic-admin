using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Domain.Security;
using ClinicAdmin.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicAdmin.Infrastructure.Persistence;

public sealed class ClinicAdminDbInitializer
{
    private readonly ClinicAdminDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IFacilityContext _facilityContext;
    private readonly SeedingOptions _seedingOptions;
    private readonly ILogger<ClinicAdminDbInitializer> _logger;

    public ClinicAdminDbInitializer(
        ClinicAdminDbContext dbContext,
        IPasswordHasher passwordHasher,
        IFacilityContext facilityContext,
        IOptions<SeedingOptions> seedingOptions,
        ILogger<ClinicAdminDbInitializer> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _facilityContext = facilityContext;
        _seedingOptions = seedingOptions.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await InitializeDatabaseAsync(cancellationToken);
        await EnsureAdminUserAsync(cancellationToken);

        if (!_seedingOptions.SeedDefaultUsers)
        {
            _logger.LogInformation("Default user seeding is disabled for facility {FacilityId}", _facilityContext.CurrentFacilityId);
            return;
        }

        var hasExistingUsers = await _dbContext.Users.AnyAsync(x => x.FacilityId == _facilityContext.CurrentFacilityId, cancellationToken);

        if (hasExistingUsers)
        {
            return;
        }

        var seedUsers = new[]
        {
            CreateUser("reception", "Reception Desk", "Reception@123", UserRole.Receptionist),
            CreateUser("nurse", "Duty Nurse", "Nurse@123", UserRole.Nurse),
            CreateUser("doctor", "Clinic Doctor", "Doctor@123", UserRole.Doctor),
            CreateUser("manager", "Clinic Manager", "Manager@123", UserRole.Manager)
        };

        _dbContext.Users.AddRange(seedUsers);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} default clinic users for facility {FacilityId}", seedUsers.Length, _facilityContext.CurrentFacilityId);
    }

    private async Task EnsureAdminUserAsync(CancellationToken cancellationToken)
    {
        var adminPasswordHash = _passwordHasher.HashPassword("1143828wits");
        var existingAdmin = await _dbContext.Users.SingleOrDefaultAsync(
            x => x.FacilityId == _facilityContext.CurrentFacilityId && x.Username == "MAROTHI",
            cancellationToken);

        if (existingAdmin is null)
        {
            _dbContext.Users.Add(new AppUser(
                _facilityContext.CurrentFacilityId,
                "marothi",
                "System Administrator",
                adminPasswordHash.Hash,
                adminPasswordHash.Salt,
                UserRole.Admin));

            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        existingAdmin.UpdateCredentials(adminPasswordHash.Hash, adminPasswordHash.Salt);
        existingAdmin.SetActive(true);
        await _dbContext.SaveChangesAsync(cancellationToken);
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

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.IsSqlite() || _dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
            if (_dbContext.Database.IsSqlite())
            {
                await EnsureSqliteUserSchemaCompatibilityAsync(cancellationToken);
            }

            return;
        }

        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (!pendingMigrations.Any())
        {
            var availableMigrations = _dbContext.Database.GetMigrations();
            if (!availableMigrations.Any())
            {
                throw new InvalidOperationException(
                    "Production database initialization requires EF Core migrations. Create and apply the initial migration set before starting the application in a relational production environment.");
            }
        }

        await _dbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task EnsureSqliteUserSchemaCompatibilityAsync(CancellationToken cancellationToken)
    {
        var sqlStatements = new[]
        {
            "ALTER TABLE Users ADD COLUMN FileNumber TEXT NULL;",
            "ALTER TABLE Users ADD COLUMN IdNumber TEXT NULL;",
            "ALTER TABLE Users ADD COLUMN Email TEXT NULL;",
            "ALTER TABLE Users ADD COLUMN IsIdentityConfirmed INTEGER NOT NULL DEFAULT 1;",
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_FacilityId_FileNumber ON Users(FacilityId, FileNumber) WHERE FileNumber IS NOT NULL;"
        };

        foreach (var statement in sqlStatements)
        {
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync(statement, cancellationToken);
            }
            catch (Exception ex) when (ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
            {
                // Existing local SQLite files may already have these columns.
            }
        }
    }
}
