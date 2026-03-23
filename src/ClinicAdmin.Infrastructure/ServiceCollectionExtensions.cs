using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Auditing;
using ClinicAdmin.Infrastructure.Auditing;
using ClinicAdmin.Infrastructure.Clock;
using ClinicAdmin.Infrastructure.Configuration;
using ClinicAdmin.Infrastructure.Persistence;
using ClinicAdmin.Infrastructure.Security;
using ClinicAdmin.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAdmin.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<AuditOptions>(configuration.GetSection(AuditOptions.SectionName));
        services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
        services.Configure<FacilityOptions>(configuration.GetSection(FacilityOptions.SectionName));
        services.Configure<SeedingOptions>(configuration.GetSection(SeedingOptions.SectionName));
        services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.AddDbContext<ClinicAdminDbContext>(options =>
        {
            if (databaseOptions.Provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase) ||
                databaseOptions.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(databaseOptions.ConnectionString);
            }
            else
            {
                options.UseSqlite(databaseOptions.ConnectionString);
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ClinicAdminDbContext>());
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<UserSessionService>();
        services.AddSingleton<ICurrentUserService>(sp => sp.GetRequiredService<UserSessionService>());
        services.AddSingleton<IUserSessionService>(sp => sp.GetRequiredService<UserSessionService>());
        services.AddSingleton<IFacilityContext, DesktopFacilityContext>();
        services.AddSingleton<IWorkstationContext, DesktopWorkstationContext>();
        services.AddSingleton<ILoginAttemptLimiter, InMemoryLoginAttemptLimiter>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ISyncJournal, SyncJournalService>();
        services.AddScoped<ClinicAdminDbInitializer>();

        return services;
    }
}
