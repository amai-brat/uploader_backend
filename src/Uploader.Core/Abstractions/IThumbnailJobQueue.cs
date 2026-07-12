namespace Uploader.Core.Abstractions;

public record ThumbnailJob(string FileName, string ContentType);

public interface IThumbnailJobQueue
{
    ValueTask EnqueueAsync(ThumbnailJob job, CancellationToken ct = default);
    
    IAsyncEnumerable<ThumbnailJob> DequeueAsync(CancellationToken ct);
}