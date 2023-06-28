using Microsoft.IdentityModel.Tokens;
using Reality.Common.Entities;
using Reality.Common.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

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
            TokenValidationResult result;
            try
            {
                result = await TokenHandler.ValidateTokenAsync(jwt, Reality.Common.Configurations.TokenConfiguration.ValidationParameters);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new TokenValidationResult()
                {
                    Exception = e,
                    IsValid = false
                };
            }

            return result;
        }

        public User? ExtractUser(TokenValidationResult validationResult)
        {
            var json = validationResult.Claims.First(x => x.Key == "user").Value;
            
            User? user;
            try
            {
                user = JsonSerializer.Deserialize<User>((string)json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
            return user;
        }

        public List<Role> ExtractRoles(TokenValidationResult validationResult)
        {
            return (ExtractUser(validationResult) ?? new()).Roles.ToList();
        }
    }
}
