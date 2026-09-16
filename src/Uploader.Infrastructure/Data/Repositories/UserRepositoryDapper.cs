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

    public async Task<User?> GetByApiKeyWithUploadsAsync(string apiKey, CancellationToken ct = default)
    {
        using var connection = context.CreateConnection();
        
        User? user = null;
        await connection.QueryAsync<User, Upload?, User>(
            """
            SELECT  u.Id, u.TwitchUserId, u.TwitchUsername, u.ApiKey,
                    up.Id,
                    up.UploadTime, up.FileId, up.OriginalFilename, up.Key,
                    up.ChecksumMd5, up.ContentType, up.Extension, up.Size,
                    up.UserAgent, up.RemoteIpAddress, up.IsDeleted, up.UserId
            FROM Users u
            LEFT JOIN Uploads up ON up.UserId = u.Id AND up.IsDeleted = 0
            WHERE u.ApiKey = @apiKey
            ORDER BY up.UploadTime DESC;
            """,
            (u, up) =>
            {
                user ??= u;
                
                if (up is null || up.Id == 0)
                    return user;

                up.UserId = user.Id;
                up.User = user;
                user.Uploads.Add(up);

                return user;
            },
            new { apiKey },
            splitOn: "Id");

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