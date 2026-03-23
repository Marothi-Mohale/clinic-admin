namespace ClinicAdmin.Infrastructure.Configuration;

public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    public bool Enabled { get; init; } = false;
    public string Mode { get; init; } = "Disabled";
}
