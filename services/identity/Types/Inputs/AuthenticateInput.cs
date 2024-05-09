using Microsoft.AspNetCore.Mvc;

namespace Foundation.Services.Identity.Types.Inputs;

public class AuthenticateInput
{
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
}