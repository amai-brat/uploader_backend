using Microsoft.AspNetCore.HttpLogging;
using System.Text.Encodings.Web;
using System.Text.Json;
using Uploader.Feature;
using Uploader.Feature.Extensions;
using Uploader.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "ENV_");

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All | HttpLoggingFields.RequestQuery;
    logging.MediaTypeOptions.AddText("application/javascript");
    logging.RequestBodyLogLimit = 4096;
    logging.ResponseBodyLogLimit = 4096;
    logging.CombineLogs = true;
});

builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss";
    options.JsonWriterOptions = new JsonWriterOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
});

builder.Services.AddOpenApi();
builder.Services.AddFeature(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseHttpLogging();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGroup("api").MapEndpoints();
app.Run();