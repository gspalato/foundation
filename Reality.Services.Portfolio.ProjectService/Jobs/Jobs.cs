using Reality.Common.Jobs;

namespace Reality.Services.Portfolio.ProjectService.Jobs
{
    public class JobsClass : IJobClass
    {
        [Job(JobType.Recurring, "0 * * * *")]
        public void RefreshProjectDatabase()
        {
            Console.WriteLine("Refresh project database job executed.");
        }
    }
}