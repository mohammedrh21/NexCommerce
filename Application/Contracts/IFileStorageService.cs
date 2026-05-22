namespace NexCommerce.Application.Contracts;

/// <summary>Cloud file storage — implemented in Infrastructure using Cloudinary.</summary>
public interface IFileStorageService
{
    /// <summary>Upload image bytes and return the public URL.</summary>
    Task<string> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Delete image by its public ID / URL.</summary>
    Task DeleteImageAsync(string imageUrlOrPublicId, CancellationToken cancellationToken = default);
}
