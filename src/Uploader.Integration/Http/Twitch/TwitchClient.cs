using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using Uploader.Integration.Core;
using Uploader.Integration.Http.Twitch.Responses;

namespace Uploader.Integration.Http.Twitch;

public class TwitchClient(HttpClient client) : ITwitchClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public async Task<ApiResponse<OAuth2ValidateResponse>> ValidateOAuth2TokenAsync(
        string token,
        CancellationToken ct = default)
    {
        const string endpoint = "oauth2/validate";

        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", token);

        var response = await client.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            var resp = await response.Content.ReadFromJsonAsync<OAuth2ValidateResponse>(JsonSerializerOptions, ct);
            return ApiResponse.CreateSuccess(resp!, (int)response.StatusCode);
        }

        var error = await response.Content.ReadFromJsonAsync<OAuth2ValidateErrorResponse>(JsonSerializerOptions, ct);
        return ApiResponse.CreateFailure<OAuth2ValidateResponse>(
            error?.Message ?? $"Error from {endpoint}" ,
            (int)response.StatusCode);
    }
}