using Microsoft.AspNetCore.Http;

namespace Uploader.Feature.Api.Upload;

public record UploadRequest(
    IFormFile? File);