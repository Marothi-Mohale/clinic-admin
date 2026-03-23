using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAdmin.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped(typeof(ValidatorExecutor<>));
        services.AddScoped<IValidator<RegisterPatientCommand>, RegisterPatientCommandValidator>();
        services.AddScoped<RegisterPatientCommandHandler>();
        services.AddSingleton<IPatientDuplicateDetectionService, PatientDuplicateDetectionService>();

        return services;
    }
}
