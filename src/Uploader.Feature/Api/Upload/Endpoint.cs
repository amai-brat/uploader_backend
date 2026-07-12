using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Uploader.Core.Abstractions;
using Uploader.Core.Options;
using Uploader.Feature.Abstractions;

namespace Uploader.Feature.Api.Upload;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class UploadEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("upload", Handle)
            .WithName("Upload")
            .DisableAntiforgery();
    }
    
    internal static async Task<Results<
        Ok<UploadResponse>,
        BadRequest<UploadErrorResponse>
    >> Handle(
        [FromForm] UploadRequest request,
        IFileStorage fileStorage,
        ILogger<UploadEndpoint> logger,
        IUploadRepository uploadRepository,
        IOptions<AppSettings> options,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var appSettings = options.Value;
        var file = request.File;

        if (file is null || file.Length == 0)
        {
            return TypedResults.BadRequest(new UploadErrorResponse("File is missing or empty"));
        }

        if (file.Length > appSettings.MaxFileSize)
        {
            return TypedResults.BadRequest(
                new UploadErrorResponse($"File size exceeds the {appSettings.MaxFileSize} bytes limit"));
        }
        
        var fileId = GenerateFileId();
        var key = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var originalFilename = file.FileName;
        var contentType = file.ContentType;
        var fileNameOnStorage = fileId + extension;

        await using var stream = file.OpenReadStream();
        var checksumMd5 = await ComputeMd5Hash(stream, ct);
        
        var storageResult = await fileStorage.UploadAsync(stream, fileNameOnStorage, ct);
        if (storageResult.IsFailure)
        {
            return TypedResults.BadRequest(new UploadErrorResponse(storageResult.ErrorMessage!));
        }

        var upload = new Core.Entities.Upload
        {
            UploadTime = DateTime.UtcNow,
            FileId = fileId,
            OriginalFilename = originalFilename,
            Key = key,
            ChecksumMd5 = checksumMd5,
            ContentType = contentType,
            Extension = extension,
            Size = file.Length,
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
            IsDeleted = false
        };

        await uploadRepository.AddAsync(upload, ct);
        await uploadRepository.SaveChangesAsync(ct);
        
        var baseUrl = appSettings.BaseUrl.TrimEnd('/');
        var link = $"{baseUrl}/{fileNameOnStorage}";
        var deleteLink = $"{baseUrl}/api/delete?key={key}";
        
        return TypedResults.Ok(new UploadResponse(
            Id: fileId,
            Extension: extension,
            ContentType: contentType,
            Checksum: checksumMd5,
            Key: key,
            Link: link,
            DeleteLink: deleteLink
        ));
    }

    private static string GenerateFileId()
    {
        const string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
        Span<char> fileIdSpan = stackalloc char[6];
        for (var i = 0; i < 6; i++)
        {
            fileIdSpan[i] = charset[RandomNumberGenerator.GetInt32(charset.Length)];
        }
        return new string(fileIdSpan);
    }

    private static async Task<string> ComputeMd5Hash(Stream stream, CancellationToken ct)
    {
        using var md5 = MD5.Create();
        var hashBytes = await md5.ComputeHashAsync(stream, ct);
        var checksumMd5 = Convert.ToHexString(hashBytes).ToLowerInvariant();
        
        return checksumMd5;
    }
}

[JsonSerializable(typeof(UploadRequest))]
[JsonSerializable(typeof(UploadResponse))]
[JsonSerializable(typeof(UploadErrorResponse))]
internal partial class UploadJsonSerializerContext : JsonSerializerContext;