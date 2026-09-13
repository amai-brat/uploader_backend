using JetBrains.Annotations;
using System.Text.Json.Serialization;
using Uploader.Feature.Api.GetObject;

namespace Uploader.Feature.Api.GetObjects;

[PublicAPI]
public record GetObjectsResponse(
    [property: JsonPropertyName("objects")]
    List<GetObjectResponse> Objects);