using System;
using System.Diagnostics;

namespace IntentionalFindings
{
    public class S6670
    {
        public void Method()
        {
            Trace.Write("Message"); // Noncompliant (S6670) {{Avoid using Trace.Write, use instead methods that specify the trace event type.}}
        }
    }
}
