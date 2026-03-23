using ClinicAdmin.Application.Abstractions;

namespace ClinicAdmin.Infrastructure.Security;

public sealed class DesktopWorkstationContext : IWorkstationContext
{
    public string WorkstationName => Environment.MachineName;
}

