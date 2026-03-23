namespace ClinicAdmin.Application.Abstractions;

public interface IFacilityContext
{
    Guid CurrentFacilityId { get; }
    string FacilityCode { get; }
}
