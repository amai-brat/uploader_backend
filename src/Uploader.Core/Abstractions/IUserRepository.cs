using Uploader.Core.Entities;

namespace Uploader.Core.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByTwitchUserIdAsync(string twitchUserId, CancellationToken ct = default);

    Task<User?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default);
    
    Task AddAsync(User user, CancellationToken ct = default);
}