using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace Infrastructure.Video;

public sealed record VideoProbeResult(int Width, int Height, double DurationSeconds);

// Shells out to ffmpeg/ffprobe if they're available on PATH. Neither dimension probing nor
// poster-frame extraction is required for a video attachment to work — if the binaries aren't
// installed, every method here just returns null/false and callers fall back to no metadata/no thumbnail.
public static class FfmpegVideoProbe
{
    private static readonly TimeSpan ProcessTimeout = TimeSpan.FromSeconds(15);

    public static async Task<VideoProbeResult?> ProbeAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var startInfo = new ProcessStartInfo("ffprobe")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("-v");
            startInfo.ArgumentList.Add("quiet");
            startInfo.ArgumentList.Add("-print_format");
            startInfo.ArgumentList.Add("json");
            startInfo.ArgumentList.Add("-show_format");
            startInfo.ArgumentList.Add("-show_streams");
            startInfo.ArgumentList.Add(filePath);

            using var process = Process.Start(startInfo);
            if (process is null) return null;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(ProcessTimeout);
            var output = await process.StandardOutput.ReadToEndAsync(cts.Token);
            await process.WaitForExitAsync(cts.Token);
            if (process.ExitCode != 0) return null;

            using var document = JsonDocument.Parse(output);
            if (!document.RootElement.TryGetProperty("streams", out var streams)) return null;

            foreach (var stream in streams.EnumerateArray())
            {
                if (!stream.TryGetProperty("codec_type", out var codecType) || codecType.GetString() != "video") continue;
                if (!stream.TryGetProperty("width", out var widthProp) || !stream.TryGetProperty("height", out var heightProp)) continue;

                var duration = 0d;
                if (document.RootElement.TryGetProperty("format", out var format) &&
                    format.TryGetProperty("duration", out var durationProp) &&
                    double.TryParse(durationProp.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDuration))
                    duration = parsedDuration;

                return new VideoProbeResult(widthProp.GetInt32(), heightProp.GetInt32(), duration);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> ExtractPosterFrameAsync(string inputPath, string outputPath, double durationSeconds, CancellationToken cancellationToken)
    {
        try
        {
            var seekSeconds = durationSeconds >= 1.5 ? 1 : 0;
            var startInfo = new ProcessStartInfo("ffmpeg")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("-y");
            startInfo.ArgumentList.Add("-ss");
            startInfo.ArgumentList.Add(TimeSpan.FromSeconds(seekSeconds).ToString(@"hh\:mm\:ss"));
            startInfo.ArgumentList.Add("-i");
            startInfo.ArgumentList.Add(inputPath);
            startInfo.ArgumentList.Add("-frames:v");
            startInfo.ArgumentList.Add("1");
            startInfo.ArgumentList.Add("-vf");
            startInfo.ArgumentList.Add("scale='min(720,iw)':-2");
            startInfo.ArgumentList.Add(outputPath);

            using var process = Process.Start(startInfo);
            if (process is null) return false;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(ProcessTimeout);
            await process.WaitForExitAsync(cts.Token);
            return process.ExitCode == 0 && File.Exists(outputPath);
        }
        catch
        {
            return false;
        }
    }
}
