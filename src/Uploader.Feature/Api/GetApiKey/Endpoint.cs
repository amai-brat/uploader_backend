using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Uploader.Core.Abstractions;
using Uploader.Core.Entities;
using Uploader.Feature.Abstractions;
using Uploader.Integration.Http.Twitch;

namespace Uploader.Feature.Api.GetApiKey;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GetApiKeyEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("key", Handle)
            .WithName("GetApiKey")
            .DisableAntiforgery();
    }
    
    internal static async Task<Results<
        Ok<GetApiKeyResponse>,
        JsonHttpResult<GetApiKeyErrorResponse>
    >> Handle(
        [FromBody] GetApiKeyRequest request,
        ITwitchClient twitchClient,
        IUserRepository userRepository,
        CancellationToken ct)
    {
        var response = await twitchClient.ValidateOAuth2TokenAsync(request.Token, ct);
        if (response.IsFailure)
        {
            return TypedResults.Json(
                new GetApiKeyErrorResponse(response.ErrorMessage),
                statusCode: response.StatusCode);
        }
        
        var user = await userRepository.GetByTwitchUserIdAsync(response.Result.UserId, ct);
        if (user is not null)
        {
            return TypedResults.Ok(new GetApiKeyResponse(user.ApiKey!));
        }

        user = new User
        {
            TwitchUserId = response.Result.UserId,
            TwitchUsername = response.Result.Login,
            ApiKey = Guid.NewGuid().ToString("N"),
        };
        await userRepository.AddAsync(user, ct);
        
        return TypedResults.Ok(new GetApiKeyResponse(user.ApiKey!));
    }
}