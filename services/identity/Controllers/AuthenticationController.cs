using Foundation.Common.Entities;
using Foundation.Common.Roles;
using Foundation.Services.Identity.Configurations;
using Foundation.Services.Identity.Services;
using Foundation.Services.Identity.Types.Inputs;
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
    [Route("auth/")]
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

    [HttpPut]
    [Authorize]
    [Route("auth/")]
    public async Task<FullUser?> RegisterAsync([FromBody] RegisterInput input)
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        if (token is null || token.Length is 0)
        {
            StatusCode(401);
            return null;
        }

        var result = await AuthorizationService.CheckAuthorizationAsync(token);
        var roles = AuthorizationService.ExtractRoles(result);

        if (!roles.Any(r => r == Role.Owner))
            return null;

        return await UserService.CreateUserAsync(input.Username, input.Password);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("auth/")]
    public async Task<AuthenticationPayload> AuthenticateAsync([FromBody] AuthenticateInput input)
    {
        try
        {
            var result = await AuthenticationService.AuthenticateAsync(input.Username, input.Password);

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