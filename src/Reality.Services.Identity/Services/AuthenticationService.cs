using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Reality.Common.Configurations;
using Reality.Common.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Reality.Common.Entities;
using Reality.Services.Identity.Payloads;

namespace Reality.Services.Identity.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationPayload> AuthenticateAsync(string username, string password);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> Logger;

        private readonly IPasswordHasher<string> Hasher;
        private readonly IUserService UserService;

        private readonly SymmetricSecurityKey JwtSecurityKey;
        private readonly JwtSecurityTokenHandler JwtTokenHandler;

        public AuthenticationService(ILogger<AuthenticationService> logger, IPasswordHasher<string> hasher,
            IUserService userService, JwtSecurityTokenHandler jwtTokenHandler)
        {
            Logger = logger;
            Hasher = hasher;
            UserService = userService;
            JwtSecurityKey = TokenConfiguration.SecurityKey;
            JwtTokenHandler = jwtTokenHandler;
        }

        public async Task<AuthenticationPayload> AuthenticateAsync(string username, string password)
        {
            try
            {
                var user = await UserService.GetUserAsync(username);
                if (user is null)
                    return new AuthenticationPayload();

                var roles = user.Roles;

                var verify = Hasher.VerifyHashedPassword(username, user.PasswordHash, password);
                Console.WriteLine(verify.ToString());
                if (verify != PasswordVerificationResult.Success)
                    return new AuthenticationPayload()
                    {
                        Error = "Wrong password."
                    };

                return new AuthenticationPayload
                {
                    Successful = true,
                    Token = GenerateAccessToken((User)user),
                    User = (User)user,
                    Error = ""
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to authenticate user.");
                return new AuthenticationPayload()
                {
                    Error = e.Message
                };
            }
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim("user", JsonSerializer.Serialize(user))
            };

            DateTime expires = user.Roles.Any(r => r is Role.Project) ? DateTime.Now.AddDays(30) : DateTime.Now.AddHours(1);

            var signingCredentials = new SigningCredentials(JwtSecurityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                "realityapibyunreaalism",
                "unreaalism",
                claims,
                expires: expires,
                signingCredentials: signingCredentials
            );

            return JwtTokenHandler.WriteToken(token);
        }
    }
}
