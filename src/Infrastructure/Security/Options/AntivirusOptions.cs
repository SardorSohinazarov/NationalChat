namespace Infrastructure.Security.Options;

public sealed class AntivirusOptions
{
    public bool Enabled { get; init; }
    public string Command { get; init; } = "clamdscan";
}
