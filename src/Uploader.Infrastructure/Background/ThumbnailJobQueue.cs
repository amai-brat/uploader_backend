using System.Threading.Channels;
using Uploader.Core.Abstractions;

namespace Uploader.Infrastructure.Background;

public class ThumbnailJobQueue : IThumbnailJobQueue
{
    private readonly Channel<ThumbnailJob> _queue;

    public ThumbnailJobQueue()
    {
        var options = new UnboundedChannelOptions { SingleReader = true };
        _queue = Channel.CreateUnbounded<ThumbnailJob>(options);
    }

    public async ValueTask EnqueueAsync(ThumbnailJob job, CancellationToken ct = default)
    {
        await _queue.Writer.WriteAsync(job, ct);
    }

    public IAsyncEnumerable<ThumbnailJob> DequeueAsync(CancellationToken ct)
    {
        return _queue.Reader.ReadAllAsync(ct);
    }
}