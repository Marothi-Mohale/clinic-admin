namespace ClinicAdmin.Infrastructure.Configuration;

public sealed class SeedingOptions
{
    public const string SectionName = "Seeding";

    public bool SeedDefaultUsers { get; init; }
}
