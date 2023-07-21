using Microsoft.Extensions.Logging;
using System;

namespace Tests.Diagnostics
{
    public class DisposableNotDisposed_ILogger
    {
        public void Tests(ILogger logger)
        {
            var scope1 = logger.BeginScope("Test");        // FN. Non-compliant {{Dispose 'scope1' when it is no longer needed.}} 
            using (var scope2 = logger.BeginScope("Test")) // Compliant
            { };
        }
    }
}
