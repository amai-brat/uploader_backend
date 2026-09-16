using System.Text.Json.Serialization;

namespace Uploader.Integration.Http.Twitch.Responses;

public class OAuth2ValidateResponse
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = null!;

    [JsonPropertyName("login")]
    public string Login { get; set; } = null!;

    [JsonPropertyName("scopes")]
    public List<string>? Scopes { get; set; }
    
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = null!;

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }
}

public class OAuth2ValidateErrorResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = null!;
}