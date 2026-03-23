using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Application.Patients.Commands.RegisterPatient;

namespace ClinicAdmin.Application.Tests.Common;

public sealed class RegisterPatientCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenNamesAreMissing_ReturnsErrors()
    {
        var validator = new RegisterPatientCommandValidator();
        var command = new RegisterPatientCommand(Guid.Empty, string.Empty, string.Empty, null, null, null, null);

        var errors = await validator.ValidateAsync(command);

        Assert.Contains(errors, error => error.PropertyName == nameof(command.FacilityId));
        Assert.Contains(errors, error => error.PropertyName == nameof(command.FirstName));
        Assert.Contains(errors, error => error.PropertyName == nameof(command.LastName));
    }
}
