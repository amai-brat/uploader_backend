using Uploader.Core.Entities;

namespace Uploader.Core.Abstractions;

public interface IUploadRepository
{
    Task AddAsync(Upload upload, CancellationToken ct = default);
    
    Task SaveChangesAsync(CancellationToken ct = default);
}