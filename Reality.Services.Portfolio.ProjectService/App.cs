using Reality.Common;
using Reality.Services.Portfolio.ProjectService.Jobs;

namespace Reality.Services.Portfolio.ProjectService
{
    public class App : IHostedService
    {
        private readonly IJobService JobService;

        public App(IJobService jobService)
        {
            JobService = jobService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Reached App#StartAsync()");
            JobService
                .RegisterJobClass<JobsClass>();

            await Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
