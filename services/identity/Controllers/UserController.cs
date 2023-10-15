using Amazon.S3;
using Amazon.S3.Model;
using Foundation.Common.Entities;
using Foundation.Common.Roles;
using Foundation.Core.SDK.API.REST;
using Foundation.Services.Identity.Configurations;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types.Payloads;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Services.Identity.Controllers;

public class UserController : Controller
{
    IIdentityConfiguration Configuration { get; set; }

    Services.IAuthenticationService AuthenticationService { get; set; }
    Core.SDK.Auth.JWT.IAuthorizationService AuthorizationService { get; set; }
    IUserService UserService { get; set; }

    IAmazonS3 S3Client { get; set; }


    public UserController(IIdentityConfiguration configuration,
    Services.IAuthenticationService authenticationService,
    Core.SDK.Auth.JWT.IAuthorizationService authorizationService,
    IUserService userService, IAmazonS3 s3Client)
    {
        Configuration = configuration;

        AuthenticationService = authenticationService;
        AuthorizationService = authorizationService;
        UserService = userService;

        S3Client = s3Client;
    }

    [HttpGet]
    [Route("user/avatar/{id}")]
    public string GetAvatar(string id)
    {
        return UserService.GetProfilePictureUrl(id);
    }

    [HttpPut]
    [Authorize]
    [Route("user/avatar/")]
    public async Task<AvatarUploadPayload> UploadAvatar(IFormFile file)
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null)
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "Invalid token."
            };

        if (file is null || file.Length is 0)
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "No file was uploaded."
            };

        // Only allow <= 5MB files.
        const int maxFileSize = 5 * 1024 * 1024;

        string[] allowedContentTypes = new string[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "Not authorized."
            };

        if (result.Claims.TryGetValue("id", out var userId))
        {
            if (file.Length > maxFileSize)
                return new AvatarUploadPayload
                {
                    Successful = false,
                    Error = "File size is too large."
                };

            if (!allowedContentTypes.Contains(file.ContentType))
                return new AvatarUploadPayload
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
                return new AvatarUploadPayload
                {
                    Successful = true
                };
            else
                return new AvatarUploadPayload
                {
                    Successful = false,
                    Error = $"Failed to upload file ({uploadResult.HttpStatusCode})."
                };
        }
        else
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
    }

    [HttpDelete]
    [Authorize]
    [Route("user/avatar/")]
    public async Task<BasePayload> DeleteAvatarAsync()
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null)
        {
            HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            return new BasePayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
        {
            HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            return new BasePayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        if (result.Claims.TryGetValue("id", out var userId))
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = Configuration.AwsFoundationIdentityProfilePictureBucket,
                Key = (string)userId
            };

            var deleteResult = await S3Client.DeleteObjectAsync(deleteObjectRequest);
            if (deleteResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                return new BasePayload
                {
                    Successful = true
                };
            }
            else
            {
                HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                return new BasePayload
                {
                    Successful = false,
                    Error = "Failed to delete file."
                };
            }
        }
        else
        {
            HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
            return new BasePayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }
    }
}