using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Uploader.Core.Options;
using Uploader.Feature.Api.Delete;
using Uploader.Feature.Api.GetObject;
using Uploader.Feature.Api.Upload;

namespace Uploader.Feature;

public static class Entry
{
    public static IServiceCollection AddFeature(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureHttpJsonOptions(static options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Add(UploadJsonSerializerContext.Default);
            options.SerializerOptions.TypeInfoResolverChain.Add(GetObjectJsonSerializerContext.Default);
            options.SerializerOptions.TypeInfoResolverChain.Add(DeleteJsonSerializerContext.Default);
        });

        services.AddOptionsWithValidateOnStart<AppSettings, AppSettingsValidateOptions>()
            .BindConfiguration(AppSettings.SectionName);
        
        return services;
    }
}