using ClinicAdmin.Application.Authentication;

namespace ClinicAdmin.Application.Tests.Authentication;

public sealed class LoginRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenUsernameAndPasswordMissing_ShouldReturnErrors()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest(string.Empty, string.Empty);

        var errors = await validator.ValidateAsync(request);

        Assert.Contains(errors, error => error.PropertyName == nameof(request.Username));
        Assert.Contains(errors, error => error.PropertyName == nameof(request.Password));
    }
}

