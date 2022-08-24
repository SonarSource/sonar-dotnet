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
            TraceMessage("my message", "MyMethod");       // Compliant "memberName" can be specified by the caller (e.g. raising OnPropertyChanged for another property in WPF)
            TraceMessage("my message");                   // Compliant
            TraceMessage("my message", filePath: "aaaa"); // Noncompliant
            //                         ^^^^^^^^^^^^^^^^
            TraceMessage("my message", lineNumber: 42);   // Noncompliant
            TraceMessage("my message",
                filePath: "aaaa",                         // Noncompliant
                lineNumber: 42);                          // Noncompliant
        }

        void PassThrough(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            TraceMessage(message, memberName, filePath, lineNumber); // Compliant
            TraceMessage(message, filePath: memberName);             // Noncompliant parameters are switched
            TraceMessage(message, memberName: filePath);             // Compliant "memberName" can be specified
        }
    }
}
