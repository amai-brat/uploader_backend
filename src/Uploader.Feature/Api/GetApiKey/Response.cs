using System.Text.Json.Serialization;

namespace Uploader.Feature.Api.GetApiKey;

public record GetApiKeyResponse(
    [property: JsonPropertyName("api_key")]
    string ApiKey);

public record GetApiKeyErrorResponse(
    [property: JsonPropertyName("error")]
    string Error);