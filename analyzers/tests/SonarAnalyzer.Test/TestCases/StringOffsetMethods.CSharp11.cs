using System;

namespace Tests.Diagnostics
{
    public class StringOffsetMethods
    {
        public StringOffsetMethods()
        {
            """Test""".Substring(1).IndexOf('t'); // Noncompliant {{Replace 'IndexOf' with the overload that accepts a startIndex parameter.}}
        }
    }
}
