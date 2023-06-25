using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Reality.Common;
using Reality.Common.Data;
using Reality.Common.Entities;
using Reality.Common.Payloads;
using Reality.Common.Roles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Reality.Services.Identity.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationPayload> AuthenticateAsync(string username, string password);
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
            Hasher = hasher;
            UserService = userService;

            JwtSecurityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("REALITY_JWT_SECURITY_KEY")!
                )
            );

            JwtTokenHandler = jwtTokenHandler;
        }

        public async Task<AuthenticationPayload> AuthenticateAsync(string username, string password)
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

        private string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(user))
            };

            claims = claims.Concat(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.ToString("d")))).ToList();

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
