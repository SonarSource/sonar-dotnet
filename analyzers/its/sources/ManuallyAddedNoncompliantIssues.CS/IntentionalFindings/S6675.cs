using System;
using System.Diagnostics;

namespace IntentionalFindings
{
    public class S6675
    {
        public void Method(TraceSwitch traceSwitch)
        {
            Trace.WriteLineIf(traceSwitch.TraceError, "Message"); // Noncompliant (S6675) {{Don't use Trace.WriteLineIf with TraceSwitch level checks.}}
        }
    }
}
