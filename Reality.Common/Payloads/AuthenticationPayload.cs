namespace Reality.Common.Payloads
{
    public class AuthenticationPayload
    {
        public bool Successful { get; set; } = false;
        public string? Token { get; set; } = null;
        public string? Error { get; set; } = "User was not found.";
    }
}
