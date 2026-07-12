using Microsoft.AspNetCore.Routing;
using Uploader.Feature.Api.Delete;
using Uploader.Feature.Api.GetObject;
using Uploader.Feature.Api.Upload;

namespace Uploader.Feature.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        UploadEndpoint.MapEndpoint(app);
        GetObjectEndpoint.MapEndpoint(app);
        DeleteEndpoint.MapEndpoint(app);
        
        return app;
    }
}