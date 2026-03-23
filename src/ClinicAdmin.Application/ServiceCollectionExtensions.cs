using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Authorization;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
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
        services.AddScoped<RegisterPatientCommandHandler>();
        services.AddSingleton<IPatientDuplicateDetectionService, PatientDuplicateDetectionService>();

        return services;
    }
}
