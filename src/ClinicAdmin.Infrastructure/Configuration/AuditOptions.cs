namespace ClinicAdmin.Infrastructure.Configuration;

public sealed class AuditOptions
{
    public const string SectionName = "Audit";

    public bool Enabled { get; init; } = true;
}
