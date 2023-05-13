using HotChocolate;
using Reality.Common;
using Reality.Common.Payloads;
using Reality.Common.Services;
using Reality.Services.Identity.Services;

namespace Reality.Services.Identity.Queries
{
    public class Query
    {
        public async Task<AuthorizationPayload> IsAuthenticatedAsync(string token, [Service] IAuthorizationService authService) {
            var result = await authService.CheckAuthorizationAsync(token);
            var roles = authService.ExtractRoles(result);

            return new() {
                Successful = result.IsValid,
                Roles = roles
            };
        }
    }
}