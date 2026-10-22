using BCIT_Social.Data;
using BCIT_Social.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
//used for getting image res
using SixLabors.ImageSharp;
using System.Security.Claims;

namespace BCIT_Social.Controllers;

[Authorize]
public class UploadService
{
    // Valid purposes/sources for file uploads
    private static readonly String[] _validPurposes = { "post", "community", "profile" };
    
    // Valid formats for each purpose
    private static readonly String[] _communityValidFormats = { ".png", ".jpg", ".jpeg", ".webp" };
    private static readonly String[] _profileValidFormats = { ".png", ".jpg", ".jpeg", ".webp" };
    private static readonly String[] _postValidFormats = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };

    // Maximum file sizes for each purpose
    private const  int PROFILE_MAX_SIZE_BYTES = 10 * 1024 * 1024; // 10 MB
    private const  int COMMUNITY_MAX_SIZE_BYTES = 10 * 1024 * 1024; // 10 MB
    private const  int POST_MAX_SIZE_BYTES = 20 * 1024 * 1024; // 20 MB


    private readonly string _containerName;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;


    public UploadService(IConfiguration configuration, 
                            ApplicationDbContext context, 
                            UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;

        ValidateAzureConfiguration(configuration);

        var accountName = configuration["AzureBlobStorage:AccountName"];
        _containerName = configuration["AzureBlobStorage:ContainerName"]!;

        _blobServiceClient = new BlobServiceClient(
            new Uri($"https://{accountName}.blob.core.windows.net"),
            new DefaultAzureCredential());
    }


    public async Task<IActionResult> Upload(IFormFile? file, String purpose, ClaimsPrincipal user,CancellationToken cancellationToken)
    {
        ValidateFile(file!, purpose);
        // image resolution check later once defined
        // try 
        // {
        //     var fileInfo = Image.Identify(file!.OpenReadStream());
        //     if (fileInfo.Width < 300 || fileInfo.Height < 300)
        //     {
        //         throw new ArgumentException("The image resolution is too low. Minimum resolution is 300x300 pixels.");
        //     }
        // }
        // // Catches an exception if the file isn't a real img or something else.
        // catch (Exception)
        // {
        //     throw new ArgumentException("An error occurred while processing the image.");
        // }

        await using var stream = file!.OpenReadStream();

        var userId = _userManager.GetUserId(user);
        var ContentType = file.ContentType;
        var blobName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blob = containerClient.GetBlobClient(blobName);

        
        // Figure out how to interface this for post, community, profile,
        _context.PostPhotos.Add(new PostPhoto
            {
                // UserId = userId,
                CreatedAt = DateTime.UtcNow,
                FileSizeBytes = file.Length,
                OriginalFileName = Path.GetFileName(file.FileName),

            });

            await _context.SaveChangesAsync();

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

        // return Ok(new
        // {
        //     blobName,
        //     sizeBytes = file.Length
        // });
    }


    /**
    * Gets the maximum file size in bytes for a given purpose.
    * @param purpose The purpose of the file (post, community, profile).
    * @return The maximum file size in bytes for the given purpose.
    * @throws NotImplementedException if the purpose is not recognized.
    */
    private int GetMaxSizeBytes(String purpose)
    {
        return purpose switch
        {
            "profile" => PROFILE_MAX_SIZE_BYTES,
            "community" => COMMUNITY_MAX_SIZE_BYTES,
            "post" => POST_MAX_SIZE_BYTES,
            _ => throw new NotImplementedException(),
        };
    }

    /**
    * Checks if the file size is valid for the given purpose.
    * @param file The file to check.
    * @param purpose The purpose of the file (post, community, profile).
    * @return True if the file size is valid for the given purpose, false otherwise.
    */
    private bool IsValidSize(IFormFile file, String purpose )
    {
        int maxSizeBytes = GetMaxSizeBytes(purpose);
        return file.Length <= maxSizeBytes;
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

        if (string.IsNullOrWhiteSpace(configuration["AzureBlobStorage:AccountName"]))
        {
            throw new InvalidOperationException(
                "AzureBlobStorage:AccountName is not configured.");
        }

        if (string.IsNullOrWhiteSpace(configuration["AzureBlobStorage:ContainerName"]))
        {
            throw new InvalidOperationException(
                "AzureBlobStorage:ContainerName is not configured.");
        }

        if (string.IsNullOrWhiteSpace(configuration["AzureBlobStorage:AccountName"]))
        {
            throw new InvalidOperationException(
                "AzureBlobStorage:AccountName read is null or empty");
        }
    }
    
    /**
    * Validates the file for upload based on its size and format.
    * @param file The file to validate.
    * @param purpose The purpose of the file (post, community, profile).
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
        if (file.Length > GetMaxSizeBytes(purpose))
        {
            throw new ArgumentException(
                $"The file is too large. Maximum size is {GetMaxSizeBytes(purpose) / (1024 * 1024)} MB.");
        }
    }

    /**
    * Checks if the file format is valid for the given purpose.
    * @param file The file to check.
    * @param purpose The purpose of the file (post, community, profile).
    * @return True if the file format is valid for the given purpose, false otherwise.
    */
    private static bool IsValidFormat(IFormFile file, String purpose)
    {
         String fileFormat = Path.GetExtension(Path.GetFileName(file.FileName));
        if (purpose == "post")
        {
            return _postValidFormats.Contains(fileFormat.ToLower());
        }
        else if (purpose == "community")
        {
            return _communityValidFormats.Contains(fileFormat.ToLower());
        }
        else if (purpose == "profile")
        {
            return _profileValidFormats.Contains(fileFormat.ToLower());
        }
        else
        {
            return false;
        }
    }
}