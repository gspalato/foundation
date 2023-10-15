using System.Net;
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
    [Route("user/{id}/avatar/")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
    public string GetAvatar(string id)
    {
        return UserService.GetProfilePictureUrl(id);
    }

    [HttpGet]
    [Route("me/avatar/")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized, Type = typeof(string))]
    public async Task<string?> GetMyAvatar()
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null)
        {
            StatusCode(401);
            return null;
        }

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
        {
            StatusCode(401);
            return null;
        }

        if (result.Claims.TryGetValue("id", out var userId))
        {
            return UserService.GetProfilePictureUrl((string)userId);
        }
        else
        {
            StatusCode(401);
            return null;
        }
    }

    [HttpPut]
    [Authorize]
    [Route("me/avatar/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AvatarUploadPayload))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(AvatarUploadPayload))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(AvatarUploadPayload))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(AvatarUploadPayload))]
    public async Task<AvatarUploadPayload> UploadAvatar(IFormFile file)
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null)
        {
            StatusCode(401);
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        if (file is null || file.Length is 0)
        {
            StatusCode(400);
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "No file was uploaded."
            };
        }

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
        {
            StatusCode(401);
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "Invalid token."
            };

        }

        if (result.Claims.TryGetValue("id", out var userId))
        {
            if (file.Length > maxFileSize)
            {
                StatusCode(400);
                return new AvatarUploadPayload
                {
                    Successful = false,
                    Error = "File size is too large."
                };

            }

            if (!allowedContentTypes.Contains(file.ContentType))
            {
                StatusCode(400);
                return new AvatarUploadPayload
                {
                    Successful = false,
                    Error = "Invalid file type. Allowed file types are JPEG, PNG, GIF and WEBP."
                };
            }

            await using Stream stream = file.OpenReadStream();

            var putObjectRequest = new PutObjectRequest
            {
                BucketName = Configuration.AwsFoundationIdentityProfilePictureBucket,
                Key = (string)userId,
                InputStream = stream,
            };

            var uploadResult = await S3Client.PutObjectAsync(putObjectRequest);
            if (uploadResult.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                StatusCode(200);
                return new AvatarUploadPayload
                {
                    Successful = true
                };
            }
            else
            {
                StatusCode(500);
                return new AvatarUploadPayload
                {
                    Successful = false,
                    Error = $"Failed to upload file ({uploadResult.HttpStatusCode})."
                };
            }
        }
        else
        {
            StatusCode(401);
            return new AvatarUploadPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }
    }

    [HttpDelete]
    [Authorize]
    [Route("me/avatar/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BasePayload))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(BasePayload))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BasePayload))]
    public async Task<BasePayload> DeleteAvatarAsync()
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null)
        {
            StatusCode(401);
            return new BasePayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
        {
            StatusCode(401);
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
                StatusCode(200);
                return new BasePayload
                {
                    Successful = true
                };
            }
            else
            {
                StatusCode(500);
                return new BasePayload
                {
                    Successful = false,
                    Error = "Failed to delete file."
                };
            }
        }
        else
        {
            StatusCode(401);
            return new BasePayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }
    }
}