using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClinicAdmin.Infrastructure.Security;

public sealed class DesktopFacilityContext : IFacilityContext
{
    private readonly FacilityOptions _options;

    public DesktopFacilityContext(IOptions<FacilityOptions> options)
    {
        _options = options.Value;
    }

    public Guid CurrentFacilityId => _options.CurrentFacilityId;

    public string FacilityCode => _options.FacilityCode;
}
