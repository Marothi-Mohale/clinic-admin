using ClinicAdmin.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ClinicAdmin.Infrastructure.Persistence;

public sealed class ClinicAdminDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ClinicAdminDbContext>
{
    public ClinicAdminDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var basePath = ResolveBasePath();
        var sharedSettingsPath = Path.Combine(basePath, "appsettings.json");

        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile(sharedSettingsPath, optional: true);

        if (!string.IsNullOrWhiteSpace(environment))
        {
            var environmentSettingsPath = Path.Combine(basePath, $"appsettings.{environment}.json");
            configurationBuilder.AddJsonFile(environmentSettingsPath, optional: true);
        }

        var configuration = configurationBuilder.Build();

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        var providerOverride = Environment.GetEnvironmentVariable("CLINICADMIN_Database__Provider");
        var connectionStringOverride = Environment.GetEnvironmentVariable("CLINICADMIN_Database__ConnectionString");

        if (!string.IsNullOrWhiteSpace(providerOverride))
        {
            databaseOptions = new DatabaseOptions
            {
                Provider = providerOverride,
                ConnectionString = databaseOptions.ConnectionString
            };
        }

        if (!string.IsNullOrWhiteSpace(connectionStringOverride))
        {
            databaseOptions = new DatabaseOptions
            {
                Provider = databaseOptions.Provider,
                ConnectionString = connectionStringOverride
            };
        }

        var optionsBuilder = new DbContextOptionsBuilder<ClinicAdminDbContext>();

        if (databaseOptions.Provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) ||
            databaseOptions.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(databaseOptions.ConnectionString);
        }
        else
        {
            optionsBuilder.UseSqlite(databaseOptions.ConnectionString);
        }

        return new ClinicAdminDbContext(optionsBuilder.Options);
    }

    private static string ResolveBasePath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var desktopProjectPath = Path.GetFullPath(Path.Combine(currentDirectory, "..", "ClinicAdmin.Desktop"));

        if (Directory.Exists(desktopProjectPath))
        {
            return desktopProjectPath;
        }

        return currentDirectory;
    }
}
