using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uploader.Core.Options;
using Uploader.Feature.Api.Upload;

namespace Uploader.Feature;

public static class Entry
{
    public static IServiceCollection AddFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureHttpJsonOptions(static options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Add(UploadJsonSerializerContext.Default);
        });

        services.AddOptionsWithValidateOnStart<AppSettings, AppSettingsValidateOptions>()
            .Bind(configuration.GetRequiredSection(AppSettings.SectionName));
        
        return services;
    }
}