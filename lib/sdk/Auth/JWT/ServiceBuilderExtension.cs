using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;


namespace Foundation.Core.SDK.Auth.JWT;

public static class ServiceBuilderExtension
{
    /// <summary>
    /// Adds JWT services to the service collection.
    /// Injects AuthorizationService as a singleton.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns>ServiceBuilder</returns>
    public static ServiceBuilder UseJWT(this ServiceBuilder builder)
    {
        builder.Configure((b) =>
        {
            b.Services.AddSingleton<IAuthorizationService, AuthorizationService>();
            b.Services.AddSingleton<AuthorizationService>();
            b.Services.AddSingleton<JwtSecurityTokenHandler>();
            b.Services
                .AddAuthorization()
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => options.TokenValidationParameters = TokenConfiguration.ValidationParameters);
        });

        return builder;
    }
}
