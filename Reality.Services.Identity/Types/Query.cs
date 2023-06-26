using HotChocolate;
using Reality.Common;
using Reality.Common.Payloads;
using Reality.Common.Services;
using Reality.Services.Identity.Services;

namespace Reality.Services.Identity.Types
{
    public class Query
    {
        public async Task<AuthorizationPayload> IsAuthenticatedAsync(string token, [Service] IAuthorizationService authService) {
            var invalidTokenError = ErrorBuilder.New()
                .SetMessage("Invalid token. Try regenerating your token.")
                .SetCode("401")
                .Build();
            
            if (token.Length < 0 || token is null)
                throw new GraphQLException(invalidTokenError);

            var result = await authService.CheckAuthorizationAsync(token);
            var roles = authService.ExtractRoles(result);

            if (!result.IsValid)
                throw new GraphQLException(invalidTokenError);

            return new() {
                Successful = result.IsValid,
                Roles = roles
            };
        }
    }
}