using ClinicAdmin.Application.Visits.Commands.RegisterVisit;
using ClinicAdmin.Domain.Visits;

namespace ClinicAdmin.Application.Tests.Visits;

public sealed class RegisterVisitCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenCompletedStateUsesNonCompletedQueue_ShouldReturnError()
    {
        var validator = new RegisterVisitCommandValidator();
        var command = new RegisterVisitCommand(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Follow up",
            QueueStatus.Waiting,
            VisitState.Completed,
            null,
            null,
            null);

        var errors = await validator.ValidateAsync(command);

        Assert.Contains(errors, x => x.PropertyName == nameof(command.QueueStatus));
    }
}
