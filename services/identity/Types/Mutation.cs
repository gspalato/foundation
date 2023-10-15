using Foundation.Common.Roles;
using Foundation.Common.Entities;
using Foundation.Services.Identity.Types.Payloads;
using Foundation.Services.Identity.Services;
using Foundation.Core.SDK.Auth.JWT;
using HotChocolate;
using Foundation.Services.Identity.Configurations;
using Amazon.S3;
using Amazon.S3.Model;

namespace Foundation.Services.Identity.Types;

public class Mutation
{
    public async Task<FullUser?> CreateUserAsync(string username, string password, string token,
        [Service] IAuthorizationService authorizationService, [Service] IUserService userService)
    {
        var result = await authorizationService.CheckAuthorizationAsync(token);
        var roles = authorizationService.ExtractRoles(result);

        if (!roles.Any(r => r == Role.Owner))
            return null;

        return await userService.CreateUserAsync(username, password);
    }

    public async Task<AuthenticationPayload> AuthenticateAsync(string username, string password,
        [Service] IAuthenticationService authService)
    {
        try
        {
            var result = await authService.AuthenticateAsync(username, password);
            Console.WriteLine(result.Error);

            return result;
        }
        catch (Exception e)
        {
            return new AuthenticationPayload
            {
                Error = e.Message
            };
        }
    }

    public async Task<bool> DeleteProfilePictureAsync(string token, [Service] IAuthorizationService authorizationService,
    [Service] IAmazonS3 s3Client, [Service] IIdentityConfiguration configuration)
    {
        var result = await authorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
            return false;

        if (result.Claims.TryGetValue("id", out var userId))
        {
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = configuration.AwsFoundationIdentityProfilePictureBucket,
                Key = (string)userId
            };

            var deleteResult = await s3Client.DeleteObjectAsync(deleteObjectRequest);
            if (deleteResult.HttpStatusCode == System.Net.HttpStatusCode.NoContent)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    public async Task<ProfilePictureUploadUrlPayload> GetProfilePictureUploadUrlAsync(string token,
    [Service] IAuthorizationService authorizationService, [Service] IUserService userService)
    {
        var result = await authorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
            return new ProfilePictureUploadUrlPayload
            {
                Successful = false,
                Error = "Not authorized."
            };

        if (result.Claims.TryGetValue("id", out var userId))
        {
            var url = await userService.GetProfilePictureUploadUrlAsync((string)userId);
            if (url is not null)
                return new ProfilePictureUploadUrlPayload
                {
                    Successful = true,
                    Url = url
                };
            else
                return new ProfilePictureUploadUrlPayload
                {
                    Successful = false,
                    Error = "Failed to get signed URL."
                };
        }
        else
            return new ProfilePictureUploadUrlPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
    }
}
