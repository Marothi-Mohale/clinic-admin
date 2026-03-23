using ClinicAdmin.Application.Abstractions;

namespace ClinicAdmin.Infrastructure.Security;

public sealed class DesktopCurrentUserService : ICurrentUserService
{
    public string Username => Environment.UserName;

    public IReadOnlyCollection<string> Roles => Array.Empty<string>();
}

