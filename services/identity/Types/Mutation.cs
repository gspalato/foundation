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
}
