using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Uploader.Core;
using Uploader.Core.Abstractions;
using Uploader.Core.Options;

namespace Uploader.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly AppSettings _appSettings;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(IOptions<AppSettings> options, ILogger<LocalFileStorage> logger)
    {
        _appSettings = options.Value;
        _logger = logger;
    }

    public async Task<Result> UploadAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        try
        {
            if (!Directory.Exists(_appSettings.StoragePath))
            {
                Directory.CreateDirectory(_appSettings.StoragePath);
            }

            var filePath = Path.Combine(_appSettings.StoragePath, fileName);

            await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            
            await stream.CopyToAsync(fs, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write file {FileName} to disk.", fileName);
            return Result.Failure("Internal storage error occurred while saving the file.");
        }
    }

    public Task<Result> DeleteAsync(string fileName, CancellationToken ct = default)
    {
        try
        {
            var filePath = Path.Combine(_appSettings.StoragePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            var thumbPath = Path.Combine(_appSettings.ThumbnailsPath, fileName);
            if (File.Exists(thumbPath))
            {
                File.Delete(thumbPath);
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FileName} from disk.", fileName);
            return Task.FromResult(Result.Failure("Internal storage error occurred while deleting the file."));
        }
    }
}