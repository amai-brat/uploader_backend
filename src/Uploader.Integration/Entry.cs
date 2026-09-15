using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Uploader.Integration.Http.Twitch;

namespace Uploader.Integration;

public static class Entry
{
    public static IServiceCollection AddIntegration(this IServiceCollection services, IConfiguration configuration)
    {
        AddTwitchClient(services);

        return services;
    }

    private static void AddTwitchClient(IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<TwitchClientOptions, TwitchClientValidateOptions>()
            .BindConfiguration(TwitchClientOptions.SectionName);

        services.AddHttpClient<ITwitchClient, TwitchClient>((sp, client) =>
        {
            var opt = sp.GetRequiredService<IOptions<TwitchClientOptions>>().Value;
            
            client.BaseAddress = new Uri(opt.BaseUrl);
        });
    }
}