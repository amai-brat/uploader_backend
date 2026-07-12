using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Uploader.Core.Abstractions;
using Uploader.Feature.Abstractions;

namespace Uploader.Feature.Api.Delete;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DeleteEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("delete", Handle)
            .WithName("Delete");
    }
    
    internal static async Task<Results<
        Ok<DeleteResponse>,
        NotFound<DeleteResponse>
    >> Handle(
        [FromQuery] string key,
        IFileStorage fileStorage,
        ILogger<DeleteEndpoint> logger,
        IUploadRepository uploadRepository,
        CancellationToken ct)
    {
        var toDeleteFilename = await uploadRepository.MarkDeletedReturningFileIdAsync(key, ct);
        if (toDeleteFilename is null)
        {
            return TypedResults.NotFound(new DeleteResponse(Success: false));
        }

        var result = await fileStorage.DeleteAsync(toDeleteFilename, ct);
        return TypedResults.Ok(new DeleteResponse(Success: result.IsSuccess));
    }
}

[JsonSerializable(typeof(DeleteResponse))]
internal partial class DeleteJsonSerializerContext : JsonSerializerContext;