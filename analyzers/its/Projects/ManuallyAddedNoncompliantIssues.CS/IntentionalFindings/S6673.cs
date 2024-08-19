using System;
using Microsoft.Extensions.Logging;

namespace IntentionalFindings
{
    public class S6673
    {
        private void Log(ILogger logger, int first, int second) =>
            logger.LogInformation("Arg: {First} {Second}", second, first); // Noncompliant (S6673)
    }
}
