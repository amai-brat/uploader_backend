using Dapper;
using Uploader.Core.Abstractions;
using Uploader.Core.Entities;

namespace Uploader.Infrastructure.Data.Repositories;

public class UserRepositoryDapper(DapperContext context) : IUserRepository
{
    public async Task<User?> GetByTwitchUserIdAsync(string twitchUserId, CancellationToken ct = default)
    {
        using var connection = context.CreateConnection();
        var user = await connection.QueryFirstOrDefaultAsync<User>("""
            SELECT Id, TwitchUserId, TwitchUsername, ApiKey
            FROM Users
            WHERE TwitchUserId = @twitchUserId
            """, new { twitchUserId });
        return user;
    }

    public async Task<User?> GetByApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        using var connection = context.CreateConnection();
        var user = await connection.QueryFirstOrDefaultAsync<User>("""
            SELECT Id, TwitchUserId, TwitchUsername, ApiKey
            FROM Users
            WHERE ApiKey = @apiKey
            """, new { apiKey });
        return user;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        using var connection = context.CreateConnection();
        var id = await connection.ExecuteScalarAsync<long>("""
            INSERT INTO Users (TwitchUserId, TwitchUsername, ApiKey)
            VALUES (@TwitchUserId, @TwitchUsername, @ApiKey)
            RETURNING Id
            """, user);
        user.Id = id;
    }
}