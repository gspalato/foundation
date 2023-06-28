using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Reality.Common.Configurations
{
    public static class TokenConfiguration
    {
        public static SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(
            new HMACSHA512(
                Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("REALITY_JWT_SECURITY_KEY")!
                )
            ).Key
        );

        public static TokenValidationParameters ValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = "realityapibyunreaalism",
                ValidAudience = "unreaalism",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = TokenConfiguration.SecurityKey
            };
    }
}