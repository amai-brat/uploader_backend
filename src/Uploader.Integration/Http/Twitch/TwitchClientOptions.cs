using Microsoft.Extensions.Options;

namespace Uploader.Integration.Http.Twitch;

public class TwitchClientOptions
{
    public const string SectionName = "Integration:Twitch";
    
    public string BaseUrl { get; set; } = null!;
}

public class TwitchClientValidateOptions : IValidateOptions<TwitchClientOptions>
{
    public ValidateOptionsResult Validate(string? name, TwitchClientOptions options)
    {
        List<string> errors = [];
        
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            errors.Add($"{TwitchClientOptions.SectionName}:{nameof(options.BaseUrl)} is required");
        
        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}