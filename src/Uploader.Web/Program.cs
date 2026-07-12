using Uploader.Feature;
using Uploader.Feature.Extensions;
using Uploader.Infrastructure;

var builder = WebApplication.CreateSlimBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables(prefix: "ENV_");

builder.Services.AddOpenApi();
builder.Services.AddFeature(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGroup("api").MapEndpoints();
app.Run();