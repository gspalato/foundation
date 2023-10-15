using Foundation.Services.Identity.Configurations;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types.Payloads;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foundation.Services.Identity.Controllers;

public class AuthenticationController : Controller
{
    IIdentityConfiguration Configuration { get; set; }

    Services.IAuthenticationService AuthenticationService { get; set; }
    Core.SDK.Auth.JWT.IAuthorizationService AuthorizationService { get; set; }
    IUserService UserService { get; set; }


    public AuthenticationController(IIdentityConfiguration configuration,
    Services.IAuthenticationService authenticationService,
    Core.SDK.Auth.JWT.IAuthorizationService authorizationService, IUserService userService)
    {
        Configuration = configuration;

        AuthenticationService = authenticationService;
        AuthorizationService = authorizationService;
        UserService = userService;
    }

    [HttpGet]
    [Authorize]
    [Route("auth")]
    public async Task<CheckAuthenticationPayload> CheckAuthenticationAsync()
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null || token.Length is 0)
        {
            StatusCode(401);
            return new CheckAuthenticationPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        if (!result.IsValid)
        {
            StatusCode(401);
            return new CheckAuthenticationPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var id = result.Claims.FirstOrDefault(c => c.Key == "id").Value.ToString();
        if (id is null || id.Length is 0)
        {
            StatusCode(401);
            return new CheckAuthenticationPayload
            {
                Successful = false,
                Error = "Invalid token."
            };
        }

        var user = await UserService.GetUserByIdAsync(id);

        return new()
        {
            Successful = result.IsValid,
            User = user
        };
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("auth/")]
    public async Task<AuthenticationPayload> AuthenticateAsync([FromBody] string username, [FromBody] string password)
    {
        try
        {
            var result = await AuthenticationService.AuthenticateAsync(username, password);

            return result;
        }
        catch (Exception e)
        {
            return new AuthenticationPayload
            {
                Error = e.Message
            };
        }
    }
}