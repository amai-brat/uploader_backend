using Microsoft.AspNetCore.Routing;

namespace Uploader.Feature.Abstractions;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}