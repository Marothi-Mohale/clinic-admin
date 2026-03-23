using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using ClinicAdmin.Application.Patients.Queries.SearchPatients;
using ClinicAdmin.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAdmin.Application.Tests.Composition;

public sealed class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplication_ShouldRegisterCoreApplicationServices()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(RegisterPatientCommandHandler));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPatientDuplicateDetectionService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IAuthorizationService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPatientRegistrationService));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IPatientSearchService));
    }
}
