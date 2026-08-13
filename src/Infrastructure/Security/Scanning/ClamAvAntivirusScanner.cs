using System.Diagnostics;
using System.ComponentModel;
using Application.Features.Files;
using Infrastructure.Security.Options;

namespace Infrastructure.Security.Scanning;

public sealed class ClamAvAntivirusScanner(AntivirusOptions options) : IAntivirusScanner
{
    public async Task<bool> IsCleanAsync(byte[] content, CancellationToken cancellationToken = default)
    {
        if (!options.Enabled) return true;
        var path = Path.Combine(Path.GetTempPath(), $"nationalchat-scan-{Guid.NewGuid():N}.upload");
        try
        {
            await System.IO.File.WriteAllBytesAsync(path, content, cancellationToken);
            var startInfo = new ProcessStartInfo
            {
                FileName = options.Command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            startInfo.ArgumentList.Add("--no-summary");
            startInfo.ArgumentList.Add(path);
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("ClamAV jarayonini ishga tushirib bo'lmadi.");
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException("ClamAV topilmadi yoki ishga tushmadi.", exception);
        }
        finally
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}
