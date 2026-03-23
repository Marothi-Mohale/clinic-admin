using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Contracts.Reports;
using Microsoft.EntityFrameworkCore;

namespace ClinicAdmin.Application.Reports.Queries;

public sealed class ReportingService : IReportingService
{
    private const string PatientRegisteredAction = "PatientRegistered";
    private const string AuthenticationAttemptAction = "AuthenticationAttempt";
    private const string VisitRegisteredAction = "VisitRegistered";
    private const string VisitUpdatedAction = "VisitUpdated";

    private readonly IApplicationDbContext _dbContext;

    public ReportingService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ClinicOperationalReportDto> GetOperationalReportAsync(ReportQueryDto query, CancellationToken cancellationToken = default)
    {
        if (query.ToDate < query.FromDate)
        {
            throw new ArgumentException("The report end date cannot be earlier than the start date.", nameof(query));
        }

        var fromUtc = query.FromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtcExclusive = query.ToDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var dailyRegistrationsRawTask = _dbContext.AuditEntries
            .AsNoTracking()
            .Where(x =>
                x.FacilityId == query.FacilityId &&
                x.Action == PatientRegisteredAction &&
                x.OccurredAtUtc >= fromUtc &&
                x.OccurredAtUtc < toUtcExclusive)
            .GroupBy(x => new { x.OccurredAtUtc.Year, x.OccurredAtUtc.Month, x.OccurredAtUtc.Day })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                group.Key.Day,
                RegistrationCount = group.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.Day)
            .ToListAsync(cancellationToken);

        var dailyVisitsRawTask = _dbContext.Visits
            .AsNoTracking()
            .Where(x =>
                x.FacilityId == query.FacilityId &&
                x.ArrivedAtUtc >= fromUtc &&
                x.ArrivedAtUtc < toUtcExclusive)
            .GroupBy(x => new { x.ArrivedAtUtc.Year, x.ArrivedAtUtc.Month, x.ArrivedAtUtc.Day })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                group.Key.Day,
                VisitCount = group.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.Day)
            .ToListAsync(cancellationToken);

        var commonReasonsTask = _dbContext.Visits
            .AsNoTracking()
            .Where(x =>
                x.FacilityId == query.FacilityId &&
                x.ArrivedAtUtc >= fromUtc &&
                x.ArrivedAtUtc < toUtcExclusive)
            .GroupBy(x => x.ReasonForVisit)
            .Select(group => new VisitReasonReportItemDto(group.Key, group.Count()))
            .OrderByDescending(x => x.VisitCount)
            .ThenBy(x => x.ReasonForVisit)
            .Take(Math.Clamp(query.TopReasons, 1, 20))
            .ToListAsync(cancellationToken);

        var patientVisitRowsTask = _dbContext.Visits
            .AsNoTracking()
            .Where(x =>
                x.FacilityId == query.FacilityId &&
                x.ArrivedAtUtc >= fromUtc &&
                x.ArrivedAtUtc < toUtcExclusive)
            .Join(
                _dbContext.Patients.AsNoTracking(),
                visit => visit.PatientId,
                patient => patient.Id,
                (visit, patient) => new
                {
                    visit.PatientId,
                    patient.PatientNumber,
                    patient.FirstName,
                    patient.LastName,
                    visit.ArrivedAtUtc,
                    visit.ReasonForVisit
                })
            .ToListAsync(cancellationToken);

        var staffAuditSummariesTask = _dbContext.AuditEntries
            .AsNoTracking()
            .Where(x =>
                x.FacilityId == query.FacilityId &&
                x.ActorUsername != null &&
                x.OccurredAtUtc >= fromUtc &&
                x.OccurredAtUtc < toUtcExclusive)
            .GroupBy(x => x.ActorUsername!)
            .Select(group => new
            {
                Username = group.Key,
                SuccessfulLogins = group.Count(x => x.Action == AuthenticationAttemptAction && x.Succeeded),
                FailedLogins = group.Count(x => x.Action == AuthenticationAttemptAction && !x.Succeeded),
                PatientRegistrations = group.Count(x => x.Action == PatientRegisteredAction),
                VisitsRegistered = group.Count(x => x.Action == VisitRegisteredAction),
                VisitsUpdated = group.Count(x => x.Action == VisitUpdatedAction),
                TotalActions = group.Count()
            })
            .OrderByDescending(x => x.TotalActions)
            .Take(Math.Clamp(query.TopStaff, 1, 25))
            .ToListAsync(cancellationToken);

        await Task.WhenAll(
            dailyRegistrationsRawTask,
            dailyVisitsRawTask,
            commonReasonsTask,
            patientVisitRowsTask,
            staffAuditSummariesTask);

        var auditSummaries = await staffAuditSummariesTask;
        var usernames = auditSummaries.Select(x => x.Username).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.FacilityId == query.FacilityId && usernames.Contains(x.Username))
            .ToDictionaryAsync(x => x.Username, cancellationToken);

        var staffActivity = auditSummaries
            .Select(summary =>
            {
                var user = users.GetValueOrDefault(summary.Username);
                return new StaffActivityReportItemDto(
                    summary.Username,
                    user?.DisplayName ?? summary.Username,
                    user?.Role.ToString() ?? "Unknown",
                    summary.SuccessfulLogins,
                    summary.FailedLogins,
                    summary.PatientRegistrations,
                    summary.VisitsRegistered,
                    summary.VisitsUpdated,
                    summary.TotalActions);
            })
            .ToArray();

        var dailyRegistrations = (await dailyRegistrationsRawTask)
            .Select(x => new DailyRegistrationReportItemDto(new DateOnly(x.Year, x.Month, x.Day), x.RegistrationCount))
            .ToArray();

        var dailyVisits = (await dailyVisitsRawTask)
            .Select(x => new DailyVisitCountReportItemDto(new DateOnly(x.Year, x.Month, x.Day), x.VisitCount))
            .ToArray();

        var patientVisitHistory = (await patientVisitRowsTask)
            .GroupBy(x => new { x.PatientId, x.PatientNumber, x.FirstName, x.LastName })
            .Select(group =>
            {
                var latestVisit = group
                    .OrderByDescending(x => x.ArrivedAtUtc)
                    .First();

                return new PatientVisitHistorySummaryReportItemDto(
                    group.Key.PatientId,
                    group.Key.PatientNumber,
                    $"{group.Key.FirstName} {group.Key.LastName}",
                    group.Count(),
                    latestVisit.ArrivedAtUtc,
                    latestVisit.ReasonForVisit);
            })
            .OrderByDescending(x => x.LastVisitAtUtc)
            .ThenByDescending(x => x.VisitCount)
            .Take(Math.Clamp(query.TopPatients, 1, 25))
            .ToArray();

        return new ClinicOperationalReportDto(
            query.FromDate,
            query.ToDate,
            dailyRegistrations.Sum(x => x.RegistrationCount),
            dailyVisits.Sum(x => x.VisitCount),
            dailyRegistrations,
            dailyVisits,
            await commonReasonsTask,
            staffActivity,
            patientVisitHistory);
    }
}
