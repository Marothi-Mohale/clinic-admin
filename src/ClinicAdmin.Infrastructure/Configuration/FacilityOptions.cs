namespace ClinicAdmin.Infrastructure.Configuration;

public sealed class FacilityOptions
{
    public const string SectionName = "Facility";

    public Guid CurrentFacilityId { get; init; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public string FacilityCode { get; init; } = "MAIN";
}
