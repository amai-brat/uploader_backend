using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace Uploader.Feature.Api.AuthGetObjects;

[PublicAPI]
public record AuthGetObjectsResponse(
    [property: JsonPropertyName("objects")]
    List<AuthGetObjectResponse> Objects);
    
[PublicAPI]
public record AuthGetObjectResponse(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("type")]
    string? ContentType,
    [property: JsonPropertyName("date")]
    long UploadDate,
    [property: JsonPropertyName("checksums")]
    AuthGetObjectChecksums Checksums,
    [property: JsonPropertyName("name")]
    string Filename,
    [property: JsonPropertyName("ext")]
    string? Extension,
    [property: JsonPropertyName("key")]
    string Key);

[PublicAPI]
public record AuthGetObjectChecksums(
    [property: JsonPropertyName("md5")]
    string Md5);