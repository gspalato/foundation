using System;
using System.Runtime.InteropServices;

namespace Foundation.Tools.Codegen;

public static class Utilities
{
    public static string CollapsePath(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return path;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (path.StartsWith(home))
            return path.Replace(home, "~");

        return path;
    }
}