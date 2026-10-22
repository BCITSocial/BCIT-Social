using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCIT_Social.Controllers;

[Authorize]
public class UploadController : Controller
{
    private const long MAX_SIZE_BYTES = 20 * 1024 * 1024; // 20 MB

    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public UploadController(IConfiguration configuration)
    {
        var accountName = configuration["AzureBlobStorage:AccountName"];
        _containerName = configuration["AzureBlobStorage:ContainerName"]
            ?? throw new InvalidOperationException(
                "AzureBlobStorage:ContainerName is not configured.");

        if (string.IsNullOrWhiteSpace(accountName))
        {
            throw new InvalidOperationException(
                "AzureBlobStorage:AccountName read is null or empty");
        }

        _blobServiceClient = new BlobServiceClient(
            new Uri($"https://{accountName}.blob.core.windows.net"),
            new DefaultAzureCredential());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Choose a file to upload.");
        }

        if (file.Length > MAX_SIZE_BYTES)
        {
            return BadRequest(
                $"The file is too large. Maximum size is {MAX_SIZE_BYTES / (1024 * 1024)} MB.");
        }

        if (Path.GetExtension(Path.GetFileName(file.FileName)) != ".png" && 
            Path.GetExtension(Path.GetFileName(file.FileName)) != ".jpg" && 
            Path.GetExtension(Path.GetFileName(file.FileName)) != ".jpeg" &&
            Path.GetExtension(Path.GetFileName(file.FileName)) != ".webp")
        {
            return BadRequest("Only .png, .jpg, .jpeg, and .webp files are allowed.");
        }

        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(
            cancellationToken: cancellationToken);

        // Use a generated name so user-supplied filenames cannot overwrite blob
        var blobName = $"{Guid.NewGuid():N}{Path.GetExtension(Path.GetFileName(file.FileName))}";
        var blob = container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();

        if (stream.Length > MAX_SIZE_BYTES)
        {
            return BadRequest(
                $"The file is too large. Maximum size is {MAX_SIZE_BYTES / (1024 * 1024)} MB.");
        }

        await blob.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType
                }
            },
            cancellationToken);

        return Ok(new
        {
            blobName,
            sizeBytes = file.Length
        });
    }
}