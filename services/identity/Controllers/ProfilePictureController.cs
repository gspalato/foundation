using Amazon.S3;
using Amazon.S3.Model;
using Foundation.Core.SDK.Auth.JWT;
using Foundation.Services.Identity.Configurations;
using Foundation.Services.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Services.Identity.Controllers;

public class ProfilePictureController : Controller
{
    IIdentityConfiguration Configuration { get; set; }

    Core.SDK.Auth.JWT.IAuthorizationService AuthorizationService { get; set; }
    IUserService UserService { get; set; }

    IAmazonS3 S3Client { get; set; }

    public ProfilePictureController(IIdentityConfiguration configuration,
    Core.SDK.Auth.JWT.IAuthorizationService authorizationService, IUserService userService,
    IAmazonS3 s3Client)
    {
        Configuration = configuration;

        AuthorizationService = authorizationService;
        UserService = userService;

        S3Client = s3Client;
    }

    [HttpGet]
    [Route("profile_picture/get/{id}")]
    public string GetProfilePicture(string id)
    {
        return UserService.GetProfilePictureUrl(id);
    }

    [HttpPost]
    [Authorize]
    [Route("profile_picture/upload")]
    public async Task<ProfilePictureUploadPayload> UploadProfilePicture(IFormFile file)
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null)
            return new ProfilePictureUploadPayload
            {
                Successful = false,
                Error = "Invalid token."
            };

        if (file is null || file.Length is 0)
            return new ProfilePictureUploadPayload
            {
                Successful = false,
                Error = "No file was uploaded."
            };

        Console.WriteLine($"RECEIVED AUTH HEADER TOKEN: {token}");

        // Only allow <= 5MB files.
        const int maxFileSize = 5 * 1_000_000;

        string[] allowedContentTypes = new string[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
            return new ProfilePictureUploadPayload
            {
                Successful = false,
                Error = "Not authorized."
            };

        if (result.Claims.TryGetValue("id", out var userId))
        {
            if (file.Length > maxFileSize)
                return new ProfilePictureUploadPayload
                {
                    Successful = false,
                    Error = "File size is too large."
                };

            if (!allowedContentTypes.Contains(file.ContentType))
                return new ProfilePictureUploadPayload
                {
                    Successful = false,
                    Error = "Invalid file type. Allowed file types are JPEG, PNG, GIF and WEBP."
                };

            await using Stream stream = file.OpenReadStream();

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = Configuration.AwsFoundationIdentityProfilePictureBucket,
                Key = (string)userId,
                InputStream = stream,
            };

            var uploadResult = await S3Client.PutObjectAsync(putObjectRequest);
            if (uploadResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
                return new ProfilePictureUploadPayload
                {
                    Successful = true
                };
            else
                return new ProfilePictureUploadPayload
                {
                    Successful = false,
                    Error = $"Failed to upload file ({uploadResult.HttpStatusCode})."
                };
        }
        else
            return new ProfilePictureUploadPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
    }
}