using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Reality.Common.Services
{
    public interface IAuthorizationService
    {
        Task<TokenValidationResult> CheckAuthorizationAsync(string jwt);
        IEnumerable<object> ExtractRoles(TokenValidationResult validationResult);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly JwtSecurityTokenHandler TokenHandler;

        public AuthorizationService(JwtSecurityTokenHandler tokenHandler)
        {
            TokenHandler = tokenHandler;
        }

        public async Task<TokenValidationResult> CheckAuthorizationAsync(string jwt)
        {
            var result = await TokenHandler.ValidateTokenAsync(jwt, new TokenValidationParameters()
            {
                ValidIssuer = "realityapibyunreaalism",
                ValidAudience = "unreaalism",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        Environment.GetEnvironmentVariable("REALITY_JWT_SECURITY_KEY") ?? "development"
                    )
                )
            });

            if (result.Exception != null)
                Console.WriteLine(result.Exception.Message);

            return result;
        }

        public IEnumerable<object> ExtractRoles(TokenValidationResult validationResult)
        {
            return validationResult.Claims.Where(x => x.Key is ClaimTypes.Role).Select(x => x.Value);
        }
    }
}
