using Uploader.Core.Abstractions;
using Uploader.Core.Entities;

namespace Uploader.Infrastructure.Data.Repositories;

public class UploadRepository(AppDbContext dbContext) : IUploadRepository
{
    public Task AddAsync(Upload upload, CancellationToken ct = default)
    {
        dbContext.Uploads.Add(upload);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}