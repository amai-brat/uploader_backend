using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Text.Json.Serialization;
using Uploader.Core.Abstractions;
using Uploader.Feature.Abstractions;

namespace Uploader.Feature.Api.GetObject;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GetObjectEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("object", Handle)
            .WithName("GetObject");
    }

    internal static async Task<Results<
        Ok<GetObjectResponse>,
        NotFound
    >> Handle(
        [FromQuery] string id,
        IUploadRepository uploadRepository,
        CancellationToken ct)
    {
        var upload = await uploadRepository.FindByFileIdAsync(id, ct);
        if (upload is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new GetObjectResponse(
            Id: id,
            ContentType: upload.ContentType,
            UploadDate: new DateTimeOffset(upload.UploadTime).ToUnixTimeMilliseconds(),
            Checksums: new GetObjectChecksums(upload.ChecksumMd5),
            Filename: upload.FileId + upload.Extension));
    }
}

[JsonSerializable(typeof(GetObjectResponse))]
[JsonSerializable(typeof(GetObjectChecksums))]
internal partial class GetObjectJsonSerializerContext : JsonSerializerContext;