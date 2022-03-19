using Hangfire;

namespace Reality.Common.Jobs
{
    public enum JobType
    {
        Once,
        Recurring
    }

    public class JobAttribute : Attribute
    {
        public JobType Type;
        public string Interval;

        public JobAttribute(JobType type, string? interval)
        {
            Type = type;
            Interval = interval ?? Cron.Hourly();
        }
    }
}
