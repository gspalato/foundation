using Foundation.Common.Roles;
using Foundation.Common.Services;
using Foundation.Common.Entities;
using Foundation.Services.Identity.Types.Payloads;
using Foundation.Services.Identity.Services;

namespace Foundation.Services.Identity.Types
{
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
    }
}
