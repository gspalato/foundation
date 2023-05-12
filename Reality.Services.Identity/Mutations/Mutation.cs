using Reality.Common;
using Reality.Common.Entities;
using Reality.Common.Payloads;
using Reality.Services.Identity.Services;

namespace Reality.Services.Identity.Mutations
{
    public class Mutation
    {
        public async Task<User?> CreateUserAsync(string username, string password, string token,
            [Service] IAuthenticationService authService, [Service] IUserService userService)
        {
            bool (isAuthenticated, claims) = authService.CheckAuthenticationAsync(token);
            var roles = claims.Where(x => x.Key == ClaimTypes.Role).Select(x => x.Value);
            
            if (!roles.Contains(Role.Owner))
                return null;

            await userService.CreateUserAsync(username, password)
        }

        public async Task<AuthenticationPayload> AuthenticateAsync(string username, string password,
            [Service] IAuthenticationService authService)
        {
            return await authService.AuthenticateAsync(username, password);
        }
    }
}
