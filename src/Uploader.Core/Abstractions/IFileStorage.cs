namespace Uploader.Core.Abstractions;

public interface IFileStorage
{
    Task<Result> UploadAsync(Stream stream, string fileName, CancellationToken ct = default);
  
    Task<Result> DeleteAsync(string fileName, CancellationToken ct = default);
}
