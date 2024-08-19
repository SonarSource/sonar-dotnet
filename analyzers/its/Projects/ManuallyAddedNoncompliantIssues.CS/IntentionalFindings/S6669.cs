using Microsoft.Extensions.Logging;

namespace IntentionalFindings
{
    public class S6669
    {
        private ILogger my_logger; // Noncompliant

        private ILogger<int> _log2; // Noncompliant

        private ILogger _logger; // Compliant
    }
}
