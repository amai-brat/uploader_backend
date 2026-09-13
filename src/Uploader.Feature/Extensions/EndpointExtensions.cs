using Microsoft.AspNetCore.Routing;
using Uploader.Feature.Api.Delete;
using Uploader.Feature.Api.GetApiKey;
using Uploader.Feature.Api.GetObjects;
using Uploader.Feature.Api.Upload;
using GetObjectEndpoint = Uploader.Feature.Api.GetObject.GetObjectEndpoint;

namespace Uploader.Feature.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        UploadEndpoint.MapEndpoint(app);
        GetObjectEndpoint.MapEndpoint(app);
        DeleteEndpoint.MapEndpoint(app);
        GetApiKeyEndpoint.MapEndpoint(app);
        GetObjectsEndpoint.MapEndpoint(app);
        
        return app;
    }
}