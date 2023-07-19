using Microsoft.IdentityModel.Tokens;
using Foundation.Common.Entities;
using Foundation.Common.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Foundation.Common.Services
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

        /// <summary>
        /// Checks if a token is valid.
        /// </summary>
        /// <param name="jwt">The JSON Web Token string.</param>
        /// <returns>A TokenValidationResult object.</returns>
        public async Task<TokenValidationResult> CheckAuthorizationAsync(string jwt)
        {
            TokenValidationResult result;
            try
            {
                result = await TokenHandler.ValidateTokenAsync(jwt, Foundation.Common.Configurations.TokenConfiguration.ValidationParameters);
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

        /// <summary>
        /// Extracts an User object from a TokenValidationResult.
        /// </summary>
        /// <param name="validationResult">The TokenValidationResult</param>
        /// <returns>An User object.</returns>
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

        /// <summary>
        /// Extracts a list of Roles from a <c>TokenValidationResult</c>.
        /// </summary>
        /// <param name="validationResult">The TokenValidationResult</param>
        /// <returns>A Role list.</returns>
        public List<Role> ExtractRoles(TokenValidationResult validationResult)
        {
            return (ExtractUser(validationResult) ?? new()).Roles.ToList();
        }
    }
}
