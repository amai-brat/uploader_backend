using Dapper;
using Uploader.Core.Abstractions;
using Uploader.Core.Entities;

namespace Uploader.Infrastructure.Data.Repositories;

public class UploadRepositoryDapper(DapperContext context) : IUploadRepository
{
    public async Task AddAsync(Upload upload, CancellationToken ct = default)
    {
        using var connection = context.CreateConnection();
        var id = await connection.ExecuteScalarAsync<long>("""
          INSERT INTO Uploads (UploadTime, FileId, OriginalFilename, "Key", 
                               ChecksumMd5, ContentType, Extension, "Size",
                               UserAgent, RemoteIpAddress, IsDeleted)
          VALUES (@UploadTime, @FileId, @OriginalFilename, @Key,
                  @ChecksumMd5, @ContentType, @Extension, @Size,
                  @UserAgent, @RemoteIpAddress, @IsDeleted)
          RETURNING Id
          """, upload);
        upload.Id = id;
    }

    public async Task<Upload?> FindByFileIdAsync(string fileId, CancellationToken ct = default)
    {
        using var connection = context.CreateConnection();
        var upload = await connection.QuerySingleAsync<Upload>("""
          SELECT Id, UploadTime, FileId, OriginalFilename, "Key", 
                 ChecksumMd5, ContentType, Extension, "Size",
                 UserAgent, RemoteIpAddress, IsDeleted
          FROM Uploads
          WHERE FileId = @fileId
          """, new { fileId });
        return upload;
    }

    public async Task<string?> MarkDeletedReturningFileIdAsync(string key, CancellationToken ct = default)
    {
        using var connection = context.CreateConnection();
        var fileId = await connection.ExecuteScalarAsync<string>("""
           UPDATE Uploads SET IsDeleted = true
           WHERE "Key" = @key
           RETURNING concat(FileId, Extension)
           """, new { key });
        return fileId;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return  Task.CompletedTask;
    }
}