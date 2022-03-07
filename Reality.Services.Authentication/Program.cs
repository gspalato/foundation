using Microsoft.AspNetCore;
using Reality.Services.Authentication;

namespace Reality.Services.Authentication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://localhost:5010", "https://localhost:5011")
                .UseStartup<Startup>()
                .Build();
        }
    }
}