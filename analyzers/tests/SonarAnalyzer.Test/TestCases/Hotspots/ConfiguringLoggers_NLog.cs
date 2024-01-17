using LogMan = NLog.LogManager;
using static NLog.LogManager;

namespace MvcApp
{
    class NLogLogging
    {
        // RSPEC-4792: https://jira.sonarsource.com/browse/RSPEC-4792
        void Foo(NLog.Config.LoggingConfiguration config)
        {
            NLog.LogManager.Configuration = config;
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}
        }


        void AdditionalTests(NLog.Config.LoggingConfiguration config)
        {
            // Call via the alias
            LogMan.Configuration = config;
//          ^^^^^^^^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}

            // Call via the "using static"
            Configuration = config;
//          ^^^^^^^^^^^^^   {{Make sure that this logger's configuration is safe.}}


            // Reading the configuration should be ok
            var current = Configuration;
            current = LogMan.Configuration;
            current = NLog.LogManager.Configuration;

            AdditionalTests(Configuration);
            AdditionalTests(LogMan.Configuration);
            AdditionalTests(NLog.LogManager.Configuration);

        }
    }
}
