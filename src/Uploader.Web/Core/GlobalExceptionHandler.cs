using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Uploader.Web.Core;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", httpContext.TraceIdentifier);
        
        var problemDetails = new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier, 
                ["timestamp"] = DateTime.UtcNow,
            },
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails, 
            ProblemDetailsJsonSerializerContext.Default.ProblemDetails, 
            "application/json", cancellationToken);
        return true;
    }
}

[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class ProblemDetailsJsonSerializerContext : JsonSerializerContext;