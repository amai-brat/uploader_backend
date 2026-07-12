using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Uploader.Core.Options;
using Uploader.Core.Abstractions;

namespace Uploader.Infrastructure.Media;

public class MediaThumbnailService
{
    private readonly AppSettings _appSettings;
    private readonly IFileStorage _fileStorage;
    private readonly ILogger<MediaThumbnailService> _logger;

    public MediaThumbnailService(
        IOptions<AppSettings> options,
        IFileStorage fileStorage,
        ILogger<MediaThumbnailService> logger)
    {
        _appSettings = options.Value;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task ProcessAsync(ThumbnailJob job, CancellationToken ct)
    {
        var sourceFilePath = Path.Combine(_appSettings.StoragePath, job.FileName);
        
        if (!File.Exists(sourceFilePath))
        {
            _logger.LogWarning("File not found for thumbnail generation: {FileName}", job.FileName);
            return;
        }

        var isImage = job.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        var isVideo = job.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);

        if (!isImage && !isVideo)
        {
            return;
        }

        await GenerateThumbnailWithFFmpegAsync(sourceFilePath, job.FileName, isVideo, ct);
    }

    private async Task GenerateThumbnailWithFFmpegAsync(string sourcePath, string fileName, bool isVideo, CancellationToken ct)
    {
        var thumbFileName = Path.ChangeExtension(fileName, ".jpg");
        var tempOutputPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");

        var videoArgs = $"-y -i \"{sourcePath}\" -ss 00:00:01.000 -vframes 1 -vf \"scale='min(320,iw)':-1\" \"{tempOutputPath}\"";
        var imageArgs = $"-y -i \"{sourcePath}\" -vf \"scale='min(320,iw)':-1\" \"{tempOutputPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = isVideo ? videoArgs : imageArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        try
        {
            process.Start();
            
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

            await process.WaitForExitAsync(timeoutCts.Token);

            if (process.ExitCode == 0 && File.Exists(tempOutputPath))
            {
                await using var fs = new FileStream(tempOutputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var storageResult = await _fileStorage.UploadThumbnailAsync(fs, thumbFileName, ct);
                
                if (storageResult.IsFailure)
                {
                    _logger.LogError("Failed to save thumbnail for {FileName} to storage.", fileName);
                }
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync(ct);
                _logger.LogError("FFmpeg failed to generate thumbnail for {FileName}. ExitCode: {ExitCode}. Error: {Error}", fileName, process.ExitCode, error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("FFmpeg process timed out or was cancelled for {FileName}", fileName);
            if (!process.HasExited)
            {
                process.Kill();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating thumbnail for {FileName}", fileName);
        }
        finally
        {
            if (File.Exists(tempOutputPath))
            {
                File.Delete(tempOutputPath);
            }
        }
    }
}