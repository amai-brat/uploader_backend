using Dapper;
using Uploader.Core.Abstractions;
using Uploader.Core.Entities;

namespace Uploader.Infrastructure.Data.Repositories;

[DapperAot]
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

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return  Task.CompletedTask;
    }
}