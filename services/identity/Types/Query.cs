using Foundation.Common;
using Foundation.Core.SDK.API.GraphQL;
using Foundation.Core.SDK.Auth.JWT;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types.Payloads;
using HotChocolate;

namespace Foundation.Services.Identity.Types;

// foundation generate query
public class Query
{
    public async Task<CheckAuthenticationPayload> CheckAuthenticationAsync(string token,
    [Service] IAuthorizationService authService, [Service] IUserService userService)
    {
        var invalidTokenError = ErrorBuilder.New()
            .SetMessage("Invalid token. Try regenerating your token.")
            .SetCode("401")
            .Build();

        if (token.Length is 0 || token is null)
            throw new GraphQLException(invalidTokenError);

        var result = await authService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
            throw new GraphQLException(invalidTokenError);

        var id = result.Claims.FirstOrDefault(c => c.Key == "id").Value.ToString();
        if (id is null || id.Length is 0)
            throw new GraphQLException(invalidTokenError);

        var user = await userService.GetUserByIdAsync(id);

        return new()
        {
            Successful = result.IsValid,
            User = user
        };
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
