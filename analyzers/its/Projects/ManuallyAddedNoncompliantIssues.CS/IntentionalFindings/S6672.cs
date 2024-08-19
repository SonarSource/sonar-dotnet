using Microsoft.Extensions.Logging;

namespace IntentionalFindings
{
    public class S6672
    {
        public S6672(ILogger<S6672> logger)
        { }

        public S6672(ILogger<int> logger) // Noncompliant
        { }
    }
}
