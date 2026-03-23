using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Authorization;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAdmin.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped(typeof(ValidatorExecutor<>));
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterPatientCommand>, RegisterPatientCommandValidator>();
        services.AddScoped<IPatientRegistrationDuplicateQueryService, PatientRegistrationDuplicateQueryService>();
        services.AddScoped<IPatientRegistrationDuplicateWarningService, PatientRegistrationDuplicateWarningService>();
        services.AddScoped<IPatientRegistrationService, RegisterPatientCommandHandler>();
        services.AddScoped<IPatientSearchService, PatientSearchService>();
        services.AddScoped<RegisterPatientCommandHandler>();
        services.AddSingleton<IPatientDuplicateDetectionService, PatientDuplicateDetectionService>();

        return services;
    }
}
