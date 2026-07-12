using JetBrains.Annotations;
using System.Text.Json.Serialization;

namespace Uploader.Feature.Api.GetObject;

[PublicAPI]
public record GetObjectResponse(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("type")]
    string? ContentType,
    [property: JsonPropertyName("date")]
    long UploadDate,
    [property: JsonPropertyName("checksums")]
    GetObjectChecksums Checksums,
    [property: JsonPropertyName("name")]
    string Filename);

[PublicAPI]
public record GetObjectChecksums(
    [property: JsonPropertyName("md5")]
    string Md5);