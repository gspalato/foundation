using NLog;
using NLog.Targets;

namespace Foundation.Common.Configurations
{
    public class LoggingConfiguration : NLog.Config.LoggingConfiguration
    {
        public LoggingConfiguration()
        {
            var console = new NLog.Targets.ColoredConsoleTarget("console");
            console.Layout = @"[${date:format=dd-MM-yyyy HH\:mm\:ss}] [${callsite:className=True:fileName=False:includeSourcePath=False:methodName=True}] ${level:format=Name}: ${message} ${onexception:inner=\n${exception:format=tostring}}";

            this.AddRule(LogLevel.Trace, LogLevel.Fatal, console);
        }
    }
}