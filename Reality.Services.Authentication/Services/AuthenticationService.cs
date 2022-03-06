using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Reality.Common;
using Reality.Common.Data;
using Reality.Common.Payloads;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Reality.Services.Authentication.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationPayload> AuthenticateAsync(string username, string password);
        Task<bool> CheckAuthenticationAsync(string jwt);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IPasswordHasher<string> Hasher;
        private readonly IUserService UserService;

        private readonly SymmetricSecurityKey JwtSecurityKey;
        private readonly JwtSecurityTokenHandler JwtTokenHandler;

        public AuthenticationService(IPasswordHasher<string> hasher,
            IUserService userService, JwtSecurityTokenHandler jwtTokenHandler)
        {
            DatabaseContext = databaseContext;
            Hasher = hasher;
            UserService = userService;

            JwtSecurityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("ADD_SECRET_HERE_WHEN_POSSIBLE_128BITS"));
            JwtTokenHandler = jwtTokenHandler;
        }

        public async Task<AuthenticationPayload> AuthenticateAsync(string username, string password)
        {
            var roles = new List<Role>();

            var found = await UserService.GetUserAsync(username);
            if (found is null)
                return new AuthenticationPayload();

            var verify = Hasher.VerifyHashedPassword(username, found.PasswordHash, password);
            Console.WriteLine(verify.ToString());
            if (verify is (PasswordVerificationResult.Success | PasswordVerificationResult.SuccessRehashNeeded))
                return new AuthenticationPayload()
                {
                    Error = "Wrong password."
                };

            return new AuthenticationPayload
            {
                Successful = true,
                Token = GenerateAccessToken(username, Guid.NewGuid().ToString(), roles.ToArray()),
                Error = ""
            };
        }

        public async Task<bool> CheckAuthenticationAsync(string jwt)
        {
            var result = await JwtTokenHandler.ValidateTokenAsync(jwt, new TokenValidationParameters()
            {
                ValidIssuer = "realityapibyunreaalism",
                ValidAudience = "unreaalism",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = JwtSecurityKey
            });

            if (result.Exception != null)
                Console.WriteLine(result.Exception.Message);

            return result!.IsValid;
        }

        private string GenerateAccessToken(string username, string userId, Role[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username)
            };

            claims = claims.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role.ToString()))).ToList();

            var signingCredentials = new SigningCredentials(JwtSecurityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                "realityapibyunreaalism",
                "unreaalism",
                claims,
                expires: DateTime.Now.AddDays(90),
                signingCredentials: signingCredentials);

            return JwtTokenHandler.WriteToken(token);
        }
    }
}
