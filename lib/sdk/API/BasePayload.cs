namespace Foundation.Core.SDK.API.REST;

public interface IBasePayload
{
    bool Successful { get; set; }

    string? Error { get; set; }
}

public class BasePayload : IBasePayload
{
    public bool Successful { get; set; }
    public string? Error { get; set; }
}