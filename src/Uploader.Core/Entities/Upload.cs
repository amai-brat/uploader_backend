namespace Uploader.Core.Entities;

public class Upload
{
    /// <summary>
    /// Upload ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Upload time (UTC)
    /// </summary>
    public DateTime UploadTime { get; set; }

    /// <summary>
    /// File ID
    /// </summary>
    public string FileId { get; set; } = null!;
    
    /// <summary>
    /// Original upload filename
    /// </summary>
    public string OriginalFilename { get; set; } = null!;

    /// <summary>
    /// Key to delete file
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// MD5
    /// </summary>
    public string ChecksumMd5 { get; set; } = null!;

    /// <summary>
    /// MIME type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// File extension
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// User-Agent of uploader
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// IP address of uploader
    /// </summary>
    public string? RemoteIpAddress { get; set; }

    /// <summary>
    /// Requested to delete
    /// </summary>
    public bool IsDeleted { get; set; }
}