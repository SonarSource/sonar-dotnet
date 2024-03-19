using System;

namespace IntentionalFindings
{
    internal class S6588
    {
        private DateTime dateTime = new DateTime(1970, 1, 1); // Noncompliant (S6588)
        private DateTimeOffset dateTimeOffset = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero); // Noncompliant
    }
}
