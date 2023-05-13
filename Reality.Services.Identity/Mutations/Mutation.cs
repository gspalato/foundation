using Reality.Common;
using Reality.Common.Entities;
using Reality.Common.Payloads;
using Reality.Common.Services;
using Reality.Services.Identity.Services;
using System.Net;
using System.Security.Claims;

namespace Reality.Services.Identity.Mutations
{
    public class Mutation
    {
        public async Task<User?> CreateUserAsync(string username, string password, string token,
            [Service] IAuthorizationService authorizationService, [Service] IUserService userService)
        {
            var result = await authorizationService.CheckAuthorizationAsync(token);
            var roles = authorizationService.ExtractRoles(result);
            
            if (!roles.Contains(Role.Owner))
                return null;

            return await userService.CreateUserAsync(username, password);
        }

        public async Task<AuthenticationPayload> AuthenticateAsync(string username, string password,
            [Service] IAuthenticationService authService)
        {
            return await authService.AuthenticateAsync(username, password);
        }
    }
}
