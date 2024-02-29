using System;
using Microsoft.Extensions.Logging;

namespace IntentionalFindings
{
    public class S6668
    {
        private void Log(ILogger logger, Exception exception) =>
            logger.LogCritical("Expected {exception}.", exception); // Noncompliant (S6668) {{Logging arguments should be passed to the correct parameter.}}
    }
}
