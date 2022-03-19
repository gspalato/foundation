using Hangfire;
using Reality.Common.Jobs;
using System.Reflection;

namespace Reality.Common
{
    public interface IJobService
    {
        void RegisterJobClass<T>() where T : class, IJobClass;
        void LoadJobsFromAssembly(Assembly assembly);
    }

    public class JobService : IJobService
    {
        private readonly List<Type> RegisteredClasses = new();

        public void RegisterJobClass<T>() where T : class, IJobClass
        {
            var classType = typeof(T);
            RegisteredClasses.Add(classType);

            foreach (var method in classType.GetMethods())
            {
                RegisterMethodAsJob(method);
            }
        }

        public void LoadJobsFromAssembly(Assembly assembly)
        {
            var jobClasses = assembly.GetTypes().Where(t => t.IsAssignableFrom(typeof(IJobClass)));

            foreach (var jobClass in jobClasses)
            {
                Console.WriteLine($"Found class {jobClass.Name} as job class.");
                foreach (var method in jobClass.GetMethods())
                {
                    RegisterMethodAsJob(method);
                }
            }
        }

        private void RegisterMethodAsJob(MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<JobAttribute>();
            if (attribute != null)
            {
                if (attribute.Type is JobType.Once)
                {
                    BackgroundJob.Enqueue(() => method.Invoke(method.DeclaringType, new object[] { }));
                }
                else if (attribute.Type is JobType.Recurring)
                {
                    RecurringJob.AddOrUpdate(method.Name,
                        () => method.Invoke(method.DeclaringType, new object[] { }), attribute.Interval);
                }

                Console.WriteLine($"Found method {method.Name} as {attribute.Type} job.");
            }
        }
    }
}
