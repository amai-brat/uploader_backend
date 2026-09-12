using Uploader.Integration.Core;
using Uploader.Integration.Http.Twitch.Responses;

namespace Uploader.Integration.Http.Twitch;

public interface ITwitchClient
{
    Task<ApiResponse<OAuth2ValidateResponse>> ValidateOAuth2TokenAsync(string token, CancellationToken ct = default);
}