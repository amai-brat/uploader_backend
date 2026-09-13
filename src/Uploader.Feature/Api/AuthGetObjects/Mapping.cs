namespace Uploader.Feature.Api.AuthGetObjects;

public static class Mapping
{
    public static AuthGetObjectResponse MapToObjectResponse(this Core.Entities.Upload upload)
    {
        var result = new AuthGetObjectResponse(
            Id: upload.FileId,
            ContentType: upload.ContentType,
            UploadDate: new DateTimeOffset(upload.UploadTime).ToUnixTimeMilliseconds(),
            Checksums: new AuthGetObjectChecksums(upload.ChecksumMd5),
            Filename: upload.FileId + upload.Extension,
            Extension: upload.Extension,
            Key: upload.Key);
        
        return result;
    }
}