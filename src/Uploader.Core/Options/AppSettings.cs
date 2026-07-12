using Microsoft.Extensions.Options;

namespace Uploader.Core.Options;

public class AppSettings
{
    public const string SectionName = "App";
    
    /// <summary>
    /// Path where to store files
    /// </summary>
    public string StoragePath { get; set; } = null!;

    /// <summary>
    /// Path where to store thumbnails
    /// </summary>
    public string ThumbnailsPath => Path.Combine(StoragePath, "t");
    
    /// <summary>
    /// Website base url
    /// </summary>
    public string BaseUrl { get; set; } = null!;

    /// <summary>
    /// Max file size in bytes
    /// </summary>
    public long MaxFileSize { get; set; } = 1024 * 1024 * 50; // 50 MiB
}


public class AppSettingsValidateOptions : IValidateOptions<AppSettings>
{
    public ValidateOptionsResult Validate(string? name, AppSettings options)
    {
        List<string> errors = [];
        
        if (string.IsNullOrWhiteSpace(options.StoragePath))
            errors.Add($"{nameof(options.StoragePath)} is required");
        
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            errors.Add($"{nameof(options.BaseUrl)} is required");
        
        return errors.Count > 0 
            ? ValidateOptionsResult.Fail(errors) 
            : ValidateOptionsResult.Success;
    }
}