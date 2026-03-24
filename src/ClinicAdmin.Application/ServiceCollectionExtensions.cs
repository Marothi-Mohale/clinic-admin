using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Authorization;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Application.Reports.Queries;
using ClinicAdmin.Application.Visits.Commands.RegisterVisit;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAdmin.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped(typeof(ValidatorExecutor<>));
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterAccountRequest>, RegisterAccountRequestValidator>();
        services.AddScoped<IValidator<RegisterPatientCommand>, RegisterPatientCommandValidator>();
        services.AddScoped<IValidator<RegisterVisitCommand>, RegisterVisitCommandValidator>();
        services.AddScoped<IValidator<UpdateVisitStateCommand>, UpdateVisitStateCommandValidator>();
        services.AddScoped<IPatientRegistrationDuplicateQueryService, PatientRegistrationDuplicateQueryService>();
        services.AddScoped<IPatientRegistrationDuplicateWarningService, PatientRegistrationDuplicateWarningService>();
        services.AddScoped<IPatientRegistrationService, RegisterPatientCommandHandler>();
        services.AddScoped<IPatientSearchService, PatientSearchService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IVisitWorkflowService, VisitWorkflowService>();
        services.AddScoped<RegisterPatientCommandHandler>();
        services.AddSingleton<IPatientDuplicateDetectionService, PatientDuplicateDetectionService>();

        return services;
    }
}
