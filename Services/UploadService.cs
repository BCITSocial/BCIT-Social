using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
//library for image resolution and format checking
using SixLabors.ImageSharp;

namespace BCIT_Social.Services;

/// <summary>
/// Minimum dimensions required for an uploaded image.
/// </summary>
public sealed record MinimumImageDimensions(int Width, int Height);

/// <summary>
/// Metadata for a file after it has been stored in Blob Storage.
/// </summary>
public sealed record UploadedFile(
    string StorageKey,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes);

public class UploadService
{
    // Valid formats for each purpose
    private static readonly String[] _communityValidFormats = { ".png", ".jpg", ".jpeg", ".webp" };
    private static readonly String[] _profileValidFormats = { ".png", ".jpg", ".jpeg", ".webp" };
    private static readonly String[] _postValidFormats = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };

    // Maximum file sizes for each purpose
    private const int PROFILE_MAX_SIZE_BYTES = 10 * 1024 * 1024; // 10 MB
    private const int COMMUNITY_MAX_SIZE_BYTES = 10 * 1024 * 1024; // 10 MB
    private const int POST_MAX_SIZE_BYTES = 20 * 1024 * 1024; // 20 MB

    // Minimum image dimensions for each purpose. lowkey this is hardcoded 300x300 for everything idk min res for specifics
    private const int MIN_IMAGE_WIDTH = 300;
    private const int MIN_IMAGE_HEIGHT = 300;

    private readonly string _containerName;
    private readonly BlobServiceClient _blobServiceClient;

    /// <summary>
    /// Description: Initializes the UploadService with Azure Blob Storage configuration.
    /// </summary>
    /// <param name="configuration">The application's configuration.</param>
    public UploadService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ValidateAzureConfiguration(configuration);

        var connectionString = configuration["AzureStorage:ConnectionString"]!;
        _containerName = configuration["AzureStorage:ContainerName"]!;

        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    /// <summary>
    /// Validates and stores an image in Blob Storage. The caller is responsible
    /// for saving the returned metadata on the appropriate database entity.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <param name="purpose">The purpose of the upload (e.g., "profilepicture", "community image", "post image").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Metadata for the stored file.</returns>
    public async Task<UploadedFile> UploadAsync(
        IFormFile? file,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file, purpose);

        var originalFileName = Path.GetFileName(file!.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var storageKey = $"{Guid.NewGuid():N}{extension}";

        await using var stream = file.OpenReadStream();
        if (!stream.CanSeek)
        {
            throw new NotSupportedException("The uploaded file stream must support seeking.");
        }

        stream.Position = 0;

        ImageInfo? imageInfo;
        try
        {
            imageInfo = await Image.IdentifyAsync(stream, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new ArgumentException("The uploaded file is not a valid image.", nameof(file), exception);
        }

        if (imageInfo is null)
        {
            throw new ArgumentException("The uploaded file is not a valid image.", nameof(file));
        }

        var detectedFormat = imageInfo.Metadata.DecodedImageFormat;
        var detectedExtensions = detectedFormat?.FileExtensions
            .Select(formatExtension => formatExtension.ToLowerInvariant());
        if (detectedFormat is null ||
            detectedExtensions is null ||
            !detectedExtensions.Contains(extension.TrimStart('.')))
        {
            throw new ArgumentException("The file extension does not match the image content.", nameof(file));
        }

        var minimumDimensions = GetMinimumImageDimensions(purpose);
        if (imageInfo.Width < minimumDimensions.Width || imageInfo.Height < minimumDimensions.Height)
        {
            throw new ArgumentException(
                $"Image must be at least {minimumDimensions.Width}x{minimumDimensions.Height} pixels.", nameof(file));
        }

        var contentType = detectedFormat.DefaultMimeType;

        stream.Position = 0;

        var blob = _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(storageKey);

        await blob.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            },
            cancellationToken);

        return new UploadedFile(
            storageKey,
            originalFileName,
            contentType,
            file.Length);
    }

    // Delete: uploaded blob
    /// <summary>
    /// Description: Deletes an uploaded image from Blob Storage.
    /// Author: Justin Chik
    /// </summary>
    /// <param name="storageKey">The key of the blob to delete.</param>
    /// <param name="cancellationToken">A token that can cancel the delete operation.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            return;
        }

        var blob = _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(storageKey);

        await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Description: Downloads an image from Blob Storage.
    /// Author: Justin Chik
    /// </summary>
    /// <param name="storageKey">The key of the blob to download.</param>
    /// <param name="cancellationToken">A token that can cancel the download operation.</param>
    /// <returns>The downloaded image content and properties.</returns>
    public async Task<BlobDownloadStreamingResult> DownloadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var blob = _blobServiceClient
            .GetBlobContainerClient(_containerName)
            .GetBlobClient(storageKey);

        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return response.Value;
    }

    /**
    * Gets the maximum file size in bytes for a given purpose.
    * @param purpose The purpose of the upload (e.g., "profilepicture", "community image", "post image").
    * @return The maximum file size in bytes.
    * @throws ArgumentException if the purpose is not recognized.
    */
    private int GetMaxSizeBytes(String purpose)
    {
        return purpose.ToLowerInvariant() switch
        {
            "profile" => PROFILE_MAX_SIZE_BYTES,
            "community" => COMMUNITY_MAX_SIZE_BYTES,
            "post" => POST_MAX_SIZE_BYTES,
            _ => throw new ArgumentException("Unknown upload purpose.", nameof(purpose)),
        };
    }

    /**
    * Checks if the file size is valid for a given purpose.
    * @param file The uploaded file.
    * @param purpose The purpose of the upload (e.g., "profilepicture", "community image", "post image").
    * @return True if the file size is valid for the given purpose, false otherwise.
    */
    private bool IsValidSize(IFormFile file, String purpose)
    {
        int maxSizeBytes = GetMaxSizeBytes(purpose);
        return file.Length <= maxSizeBytes;
    }

    private static MinimumImageDimensions GetMinimumImageDimensions(String purpose)
    {
        return purpose.ToLowerInvariant() switch
        {
            "profile" => new MinimumImageDimensions(MIN_IMAGE_WIDTH, MIN_IMAGE_HEIGHT),
            "community" => new MinimumImageDimensions(MIN_IMAGE_WIDTH, MIN_IMAGE_HEIGHT),
            "post" => new MinimumImageDimensions(MIN_IMAGE_WIDTH, MIN_IMAGE_HEIGHT),
            _ => throw new ArgumentException("Unknown upload purpose.", nameof(purpose))
        };
    }

    /**
    * Validates the Azure Blob Storage configuration.
    * @param configuration The application configuration.
    * @throws ArgumentNullException if the configuration is null.
    * @throws InvalidOperationException if any required configuration is missing or invalid.
    */
    private static void ValidateAzureConfiguration(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration["AzureStorage:ConnectionString"]))
        {
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is not configured.");
        }

        if (string.IsNullOrWhiteSpace(configuration["AzureStorage:ContainerName"]))
        {
            throw new InvalidOperationException(
                "AzureStorage:ContainerName is not configured.");
        }
    }

    /**
    * Validates the file for upload based on its size and format.
    * @param file The file to validate.
    * @param purpose The purpose of the upload (e.g., "profilepicture", "community image", "post image").
    * @throws ArgumentException if the file is null, empty, has an invalid format, or exceeds the maximum size for the given purpose.
    */
    private void ValidateFile(IFormFile file, String purpose)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Choose a file to upload.");
        }
        if (!IsValidFormat(file, purpose))
        {
            throw new ArgumentException("Invalid file format.");
        }
        if (!IsValidSize(file, purpose))
        {
            throw new ArgumentException(
                $"The file is too large. Maximum size is {GetMaxSizeBytes(purpose) / (1024 * 1024)} MB.");
        }
    }

    /**
    * Checks if the file format is valid for a given purpose.
    * @param file The uploaded file.
    * @param purpose The purpose of the upload (e.g., "profilepicture", "community image", "post image").
    * @return True if the file format is valid for the given purpose, false otherwise.
    */
    private static bool IsValidFormat(IFormFile file, String purpose)
    {
        String fileFormat = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
        String normalizedPurpose = purpose.ToLowerInvariant();
        if (normalizedPurpose == "post")
        {
            return _postValidFormats.Contains(fileFormat);
        }
        else if (normalizedPurpose == "community")
        {
            return _communityValidFormats.Contains(fileFormat);
        }
        else if (normalizedPurpose == "profile")
        {
            return _profileValidFormats.Contains(fileFormat);
        }
        else
        {
            return false;
        }
    }

}
