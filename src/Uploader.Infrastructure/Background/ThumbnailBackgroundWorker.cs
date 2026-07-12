using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uploader.Core.Abstractions;

namespace Uploader.Infrastructure.Background;

public class ThumbnailBackgroundWorker : BackgroundService
{
    private readonly IThumbnailJobQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ThumbnailBackgroundWorker> _logger;

    public ThumbnailBackgroundWorker(
        IThumbnailJobQueue queue, 
        IServiceProvider serviceProvider, 
        ILogger<ThumbnailBackgroundWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Thumbnail Background Worker is starting.");

        await foreach (var job in _queue.DequeueAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var thumbnailService = scope.ServiceProvider.GetRequiredService<Media.MediaThumbnailService>();
                
                await thumbnailService.ProcessAsync(job, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred processing thumbnail for {FileName}.", job.FileName);
            }
        }
    }
}