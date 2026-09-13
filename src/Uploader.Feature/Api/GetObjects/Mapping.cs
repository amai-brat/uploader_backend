using Uploader.Feature.Api.GetObject;

namespace Uploader.Feature.Api.GetObjects;

public static class Mapping
{
    public static GetObjectResponse MapToObjectResponse(this Core.Entities.Upload upload)
    {
        var result = new GetObjectResponse(
            Id: upload.Id.ToString(),
            ContentType: upload.ContentType,
            UploadDate: new DateTimeOffset(upload.UploadTime).ToUnixTimeMilliseconds(),
            Checksums: new GetObjectChecksums(upload.ChecksumMd5),
            Filename: upload.FileId + upload.Extension);
        
        return result;
    }
}