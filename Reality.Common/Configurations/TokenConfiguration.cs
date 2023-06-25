using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Reality.Common.Configurations
{
    public static class TokenConfiguration
    {
        public static TokenValidationParameters ValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = "realityapibyunreaalism",
                ValidAudience = "unreaalism",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        Environment.GetEnvironmentVariable("REALITY_JWT_SECURITY_KEY") ?? "insecure_placeholder"
                    )
                )
            };
    }
}