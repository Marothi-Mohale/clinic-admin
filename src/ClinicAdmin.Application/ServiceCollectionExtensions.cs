using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAdmin.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped(typeof(ValidatorExecutor<>));
        services.AddScoped<IValidator<RegisterPatientCommand>, RegisterPatientCommandValidator>();
        services.AddScoped<RegisterPatientCommandHandler>();

        return services;
    }
}
