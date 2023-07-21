using Microsoft.Extensions.Logging;
using System;

namespace Tests.Diagnostics
{
    public class DisposableNotDisposed_ILogger
    {
        public void Tests(ILogger logger)
        {
            var scope1 = logger.BeginScope("Test");                                // FN. Non-compliant {{Dispose 'scope1' when it is no longer needed.}} 
            using (var scope2 = logger.BeginScope("Test"))                         // Compliant
            { };
            var scope3 = logger.BeginScope<string>(null);                          // FN
            var scope4 = logger.BeginScope("messageFormat", 1);                    // FN. extension method
            var scope5 = LoggerExtensions.BeginScope(logger, "messageFormat", 1);  // FN. extension method
        }
    }
}
