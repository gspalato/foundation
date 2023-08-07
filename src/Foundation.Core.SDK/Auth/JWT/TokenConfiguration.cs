using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Foundation.Core.SDK.Auth.JWT;

public static class TokenConfiguration
{
    public static SymmetricSecurityKey SecurityKey { get; } = new SymmetricSecurityKey(
        new HMACSHA512(
            Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("FOUNDATION_JWT_SECURITY_KEY")!
            )
        ).Key
    );

    public static TokenValidationParameters ValidationParameters { get; } = new TokenValidationParameters()
    {
        ValidIssuer = "foundationapibyunreaalism",
        ValidAudience = "unreaalism",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = SecurityKey
    };
}
