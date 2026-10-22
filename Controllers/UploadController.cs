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

namespace BCIT_Social.Controllers;

[Authorize]
public class UploadController : Controller
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    private UploadService _uploadService;



    public UploadController(IConfiguration configuration,
                            ApplicationDbContext context,
                            UserManager<ApplicationUser> userManager)
    {
        _uploadService = new UploadService(configuration, context, userManager);

    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile? file, String purpose, ApplicationUser user, CancellationToken cancellationToken)
    {
        if (_uploadService == null)
        {
            return BadRequest("Upload service is not initialized.");
        }
        else
        {

            return await _uploadService.Upload(file, purpose, user, cancellationToken);
        }

    }
}
