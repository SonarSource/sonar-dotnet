using Microsoft.Extensions.Logging;

namespace IntentionalFindings
{
    public class S6664
    {
        public void Method(ILogger logger)
        {
            logger.LogError("Error 1");     // Noncompliant (S6664)
            logger.LogError("Error 2");
        }
    }
}
