using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexCommerce.Application.Contracts;

namespace NexCommerce.Infrastructure.Services;

/// <summary>
/// Cloud image storage via Cloudinary, implementing <see cref="IFileStorageService"/>.
/// Configure <c>Cloudinary:CloudName</c>, <c>Cloudinary:ApiKey</c>, <c>Cloudinary:ApiSecret</c>
/// in appsettings.Development.json (never committed to source control).
/// </summary>
public sealed class FileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration config, ILogger<FileStorageService> logger)
    {
        _logger = logger;

        var cloudName = config["Cloudinary:CloudName"] ?? throw new InvalidOperationException("Cloudinary:CloudName is not configured.");
        var apiKey    = config["Cloudinary:ApiKey"]    ?? throw new InvalidOperationException("Cloudinary:ApiKey is not configured.");
        var apiSecret = config["Cloudinary:ApiSecret"] ?? throw new InvalidOperationException("Cloudinary:ApiSecret is not configured.");

        _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret))
        {
            Api = { Secure = true }
        };
    }

    /// <inheritdoc/>
    public async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        // Sanitize filename — remove path traversal risk
        var safeFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));

        var uploadParams = new ImageUploadParams
        {
            File          = new FileDescription(safeFileName, imageStream),
            Folder        = "nexcommerce",
            UseFilename   = false,
            UniqueFilename = true,
            Overwrite     = false,
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
        {
            _logger.LogError("Cloudinary upload error for {FileName}: {Error}", fileName, result.Error.Message);
            throw new InvalidOperationException($"Image upload failed: {result.Error.Message}");
        }

        _logger.LogInformation("Image uploaded → {Url}", result.SecureUrl);
        return result.SecureUrl.ToString();
    }

    /// <inheritdoc/>
    public async Task DeleteImageAsync(
        string imageUrlOrPublicId,
        CancellationToken cancellationToken = default)
    {
        // Derive Cloudinary public ID from a full URL if necessary
        var publicId = ExtractPublicId(imageUrlOrPublicId);

        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        _logger.LogInformation(
            "Cloudinary destroy {PublicId} → {Result}", publicId, result.Result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static string ExtractPublicId(string imageUrlOrPublicId)
    {
        // If it looks like a URL, strip the host + extension to get the Cloudinary public ID
        if (!imageUrlOrPublicId.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return imageUrlOrPublicId;

        // e.g. https://res.cloudinary.com/<cloud>/image/upload/v123/nexcommerce/abc.jpg
        //   → nexcommerce/abc
        var uri      = new Uri(imageUrlOrPublicId);
        var segments = uri.AbsolutePath.Split('/');

        // Find "upload" segment index, then take everything after (minus version segment if present)
        var uploadIdx = Array.IndexOf(segments, "upload");
        if (uploadIdx < 0 || uploadIdx >= segments.Length - 1)
            return imageUrlOrPublicId;

        var afterUpload = segments[(uploadIdx + 1)..];

        // Drop version segment (v12345)
        if (afterUpload.Length > 0 && afterUpload[0].StartsWith("v") && long.TryParse(afterUpload[0][1..], out _))
            afterUpload = afterUpload[1..];

        var joined = string.Join("/", afterUpload);
        return Path.ChangeExtension(joined, null); // remove .jpg / .png etc.
    }
}
