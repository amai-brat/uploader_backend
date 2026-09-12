namespace Uploader.Core.Entities;

public class User
{
    /// <summary>
    /// User ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// User ID from Twitch
    /// </summary>
    public required string TwitchUserId { get; set; }

    /// <summary>
    /// Username from Twitch
    /// </summary>
    public required string TwitchUsername { get; set; }

    /// <summary>
    /// ApiKey
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Uploads created using ApiKey
    /// </summary>
    public List<Upload> Uploads { get; set; } = [];
}