using System.Text.Json.Serialization;

namespace Uploader.Feature.Api.Upload;

public record UploadResponse(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("ext")]
    string Extension,
    [property: JsonPropertyName("type")]
    string ContentType,
    [property: JsonPropertyName("checksum")]
    string Checksum,
    [property: JsonPropertyName("key")]
    string Key,
    [property: JsonPropertyName("link")]
    string Link,
    [property: JsonPropertyName("delete")]
    string DeleteLink);

public record UploadErrorResponse(
    [property: JsonPropertyName("error")]
    string Error);