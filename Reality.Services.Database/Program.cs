using Microsoft.AspNetCore;

namespace Reality.Services.Database
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
                .UseUrls("http://localhost:5020", "https://localhost:5021")
                .UseStartup<Startup>()
                .Build();
        }
    }
}