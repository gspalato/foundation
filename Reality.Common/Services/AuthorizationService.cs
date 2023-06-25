using Microsoft.IdentityModel.Tokens;
using Reality.Common.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Reality.Common.Services
{
    public interface IAuthorizationService
    {
        Task<TokenValidationResult> CheckAuthorizationAsync(string jwt);
        List<Role> ExtractRoles(TokenValidationResult validationResult);
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
            var result = await TokenHandler.ValidateTokenAsync(jwt, Reality.Common.Configurations.TokenConfiguration.ValidationParameters);

            if (result.Exception != null)
                Console.WriteLine(result.Exception.Message);

            return result;
        }

        public List<Role> ExtractRoles(TokenValidationResult validationResult)
        {
            return validationResult.Claims.Where(x => x.Key is ClaimTypes.Role).Select(x => {
                int enumValue = Int32.Parse((string)x.Value);
                Role role = (Role)enumValue;

                return role;
            }).ToList();
        }
    }
}
