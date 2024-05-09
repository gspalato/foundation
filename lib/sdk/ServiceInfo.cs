using System.Diagnostics;

namespace Foundation.Core.SDK;

public class ServiceInfo
{
    /// <summary>
    ///   The name of the service.
    /// </summary>
    public string Name { get; internal set; } = "Unnamed";

    /// <summary>
    ///   The unix timestamp when the service was started using the <see cref="ServiceBuilder"/>.Run() method.
    /// </summary>
    public long StartTime { get; init; } = new DateTimeOffset(Process.GetCurrentProcess().StartTime).ToUnixTimeSeconds();

    /// <summary>
    ///   The uptime of the service in seconds.
    ///   This is calculated by subtracting the <see cref="StartTime"/> from the current unix timestamp.
    /// </summary>
    public long Uptime => DateTimeOffset.UtcNow.ToUnixTimeSeconds() - StartTime;
}