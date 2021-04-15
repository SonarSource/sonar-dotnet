using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    class ArgumentSpecifiedForCallerInfoParameter
    {
        void TraceMessage(string message,
          [CallerMemberName] string memberName = "",
          [CallerFilePath] string filePath = "",
          [CallerLineNumber] int lineNumber = 0)
        {
            /* ... */
        }

        void MyMethod()
        {
            TraceMessage("my message", "MyMethod"); // Compliant
            TraceMessage("my message"); // Compliant
            TraceMessage("my message", filePath: "aaaa"); // Noncompliant
            TraceMessage("my message", lineNumber: 42); // Noncompliant
            TraceMessage("my message",
                filePath: "aaaa",  // Noncompliant
                lineNumber: 42); // Noncompliant
        }
    }
}
