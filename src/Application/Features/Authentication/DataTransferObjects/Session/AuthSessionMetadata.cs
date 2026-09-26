namespace Application.Features.Authentication.DataTransferObjects.Session;

public sealed record AuthSessionMetadata(
    string DeviceName,
    string SystemVersion,
    string AppVersion,
    string IpAddress,
    string? UserAgent)
{
    /// <summary>
    /// Refresh token this browser already had when it signed in again. Its session belongs to the same device and is
    /// replaced by the new one, so the device list does not grow with every sign-in.
    /// </summary>
    public string? ReplacedRefreshToken { get; init; }

    // Keeps the refresh token out of ToString(), and so out of any log line that prints this record.
    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append($"DeviceName = {DeviceName}, SystemVersion = {SystemVersion}, AppVersion = {AppVersion}, IpAddress = {IpAddress}, UserAgent = {UserAgent}");
        return true;
    }
}
