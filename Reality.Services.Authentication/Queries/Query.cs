using Reality.Services.Authentication.Services;

namespace Reality.Services.Authentication.Queries
{
    public class Query
    {
        public async Task<bool> IsAuthenticatedAsync(string token, [Service] IAuthenticationService authService) =>
            await authService.CheckAuthenticationAsync(token);
    }
}