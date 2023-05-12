using Reality.Services.Identity.Services;

namespace Reality.Services.Identity.Queries
{
    public class Query
    {
        public async Task<(bool, Dictionary<string, object>)> IsAuthenticatedAsync(string token, [Service] IAuthenticationService authService) =>
            await authService.CheckAuthenticationAsync(token);
    }
}