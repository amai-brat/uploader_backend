using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Uploader.Core.Abstractions;
using Uploader.Feature.Abstractions;

namespace Uploader.Feature.Api.GetObjects;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GetObjectsEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("uploads", Handle)
            .WithName("GetUploads");
    }

    internal static async Task<Results<
        Ok<GetObjectsResponse>,
        UnauthorizedHttpResult
    >> Handle(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return TypedResults.Unauthorized();
        
        var user = await userRepository.GetByApiKeyWithUploadsAsync(apiKey, ct);
        if (user is null)
            return TypedResults.Unauthorized();

        var result = new GetObjectsResponse(user.Uploads
            .Select(x => x.MapToObjectResponse())
            .ToList());
        
        return TypedResults.Ok(result);
    }
}