using Reality.Common.Payloads;
using Reality.Common.Services;
using Reality.Services.Identity.Services;

namespace Reality.Services.Identity.Types
{
    public class Query
    {
        public async Task<CheckAuthenticationPayload> CheckAuthenticationAsync(string token, [Service] IAuthorizationService authService, [Service] IUserService userService)
        {
            var invalidTokenError = ErrorBuilder.New()
                .SetMessage("Invalid token. Try regenerating your token.")
                .SetCode("401")
                .Build();

            if (token.Length < 0 || token is null)
                throw new GraphQLException(invalidTokenError);

            var result = await authService.CheckAuthorizationAsync(token);
            var id = result.Claims.FirstOrDefault(c => c.Key == "id").Value.ToString();
            if (id is null || id.Length is 0)
                throw new GraphQLException(invalidTokenError);

            var user = await userService.GetUserByIdAsync(id);

            if (!result.IsValid)
                throw new GraphQLException(invalidTokenError);

            return new()
            {
                Successful = result.IsValid,
                User = user
            };
        }
    }
}