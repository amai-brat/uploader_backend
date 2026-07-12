using Microsoft.AspNetCore.Routing;
using Uploader.Feature.Api.Upload;

namespace Uploader.Feature.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        UploadEndpoint.MapEndpoint(app);
        
        return app;
    }
}