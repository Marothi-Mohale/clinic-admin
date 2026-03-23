using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Infrastructure.Auditing;
using ClinicAdmin.Infrastructure.Clock;
using ClinicAdmin.Infrastructure.Configuration;
using ClinicAdmin.Infrastructure.Persistence;
using ClinicAdmin.Infrastructure.Security;
using ClinicAdmin.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ClinicAdmin.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<AuditOptions>(configuration.GetSection(AuditOptions.SectionName));
        services.Configure<FacilityOptions>(configuration.GetSection(FacilityOptions.SectionName));
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
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<ICurrentUserService, DesktopCurrentUserService>();
        services.AddSingleton<IFacilityContext, DesktopFacilityContext>();
        services.AddScoped<ISyncJournal, SyncJournalService>();

        return services;
    }
}
