using NLog;
using NLog.Targets;

namespace Reality.Common.Configurations
{
    public class LoggingConfiguration : NLog.Config.LoggingConfiguration
    {
        public LoggingConfiguration()
        {
            var console = new NLog.Targets.ColoredConsoleTarget("console");
            console.Layout = @"[${date:format=dd-MM-yyyy HH\:mm\:ss}] ${level:format=Name}: ${message} ${onexception:inner=\n${exception:format=tostring}}}";

            var rowRules = console.RowHighlightingRules;
            var wordRules = console.WordHighlightingRules;
            var unchanged = ConsoleOutputColor.NoChange;

            wordRules.Add(new ConsoleWordHighlightingRule("Trace:", ConsoleOutputColor.Green, unchanged));
            wordRules.Add(new ConsoleWordHighlightingRule("Debug:", ConsoleOutputColor.Green, unchanged));
            wordRules.Add(new ConsoleWordHighlightingRule("Info:", ConsoleOutputColor.Blue, unchanged));
            wordRules.Add(new ConsoleWordHighlightingRule("Warn:", ConsoleOutputColor.Yellow, unchanged));
            wordRules.Add(new ConsoleWordHighlightingRule("Error:", ConsoleOutputColor.DarkRed, unchanged));
            wordRules.Add(new ConsoleWordHighlightingRule("Fatal:", ConsoleOutputColor.Red, unchanged));

            this.AddRule(LogLevel.Trace, LogLevel.Fatal, console);
        }
    }
}