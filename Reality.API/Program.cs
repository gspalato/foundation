using Microsoft.AspNetCore;
using Reality.API;

namespace Reality
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
                .UseUrls("http://localhost:5000", "https://localhost:5001")
                .UseStartup<Startup>()
                .Build();
        }
    }
}