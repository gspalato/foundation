using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Reality.Common;
using Reality.Common.Entities;
using Reality.Common.Payloads;
using Reality.Common.Roles;
using Reality.Common.Services;
using Reality.Services.Identity.Services;

namespace Reality.Services.Identity.Types
{
    public class Mutation
    {
        public async Task<Reality.Common.Entities.FullUser?> CreateUserAsync(string username, string password, string token,
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
            var result = await authService.AuthenticateAsync(username, password);
            Console.WriteLine(result.Error);

            return result;
        }
    }
}
