using ClinicAdmin.Application.Patients.Commands.RegisterPatient;
using ClinicAdmin.Application.Patients.DuplicateDetection;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicAdmin.Application.Tests.Composition;

public sealed class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplication_ShouldRegisterCoreApplicationServices()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<RegisterPatientCommandHandler>());
        Assert.NotNull(provider.GetService<IPatientDuplicateDetectionService>());
    }
}
