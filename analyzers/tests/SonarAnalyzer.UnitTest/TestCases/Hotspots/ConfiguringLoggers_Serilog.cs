using Serilog;
using Serilog.Core;

namespace Logging
{
    class SerilogLogging
    {
        // RSPEC-4792: https://jira.sonarsource.com/browse/RSPEC-4792
        void Foo()
        {
            new Serilog.LoggerConfiguration();
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}
        }

        void AdditionalTests()
        {
            var config = new LoggerConfiguration(); // Noncompliant
            config = new MyConfiguration();         // Noncompliant

            // Using the logger shouldn't raise issues
            var levelSwitch = new LoggingLevelSwitch();
            levelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Warning;


            var newLog = config.
                MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger = newLog;
            Log.Information("logged info");
            Log.CloseAndFlush();
        }
    }

    class MyConfiguration : LoggerConfiguration { }
}
