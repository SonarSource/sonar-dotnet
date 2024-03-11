using System.Diagnostics;
using AliasedTrace = System.Diagnostics.Trace;
using AliasedTraceSwitch = System.Diagnostics.TraceSwitch;

public class Program
{
    private TraceSwitch _traceSwitch;

    public void Noncompliant_TraceIfMethods(bool condition)
    {
        Trace.WriteIf(condition, "Message");                    // Compliant
        Trace.WriteLineIf(condition, "Message");

        Trace.WriteIf(_traceSwitch.TraceError, "Message");      // Noncompliant {{'Trace.WriteIf' should not be used with 'TraceSwitch' levels.}}
        //            ^^^^^^^^^^^^^^^^^^^^^^^
        Trace.WriteLineIf(_traceSwitch.TraceError, "Message");  // Noncompliant {{'Trace.WriteLineIf' should not be used with 'TraceSwitch' levels.}}
    }

    public void Compliant_TraceMethods(string arg)
    {
        Trace.Write("Message");
        Trace.Write("Message", "Category");

        Trace.WriteLine("Message");
        Trace.WriteLine("Message", "Category");

        Trace.TraceError("Message");
        Trace.TraceError("Message: {0}", arg);

        Trace.TraceInformation("Message");
        Trace.TraceInformation("Message: {0}", arg);

        Trace.TraceWarning("Message");
        Trace.TraceWarning("Message: {0}", arg);
    }

    public void WriteIf_Overloads(bool condition)
    {
        Trace.WriteIf(_traceSwitch.TraceError, "Message");                  // Noncompliant
        Trace.WriteIf(_traceSwitch.TraceError, 42);                         // Noncompliant
        Trace.WriteIf(_traceSwitch.TraceError, "Message", "Category");      // Noncompliant
        Trace.WriteIf(_traceSwitch.TraceError, 42, "Category");             // Noncompliant

        Trace.WriteLineIf(_traceSwitch.TraceError, "Message");              // Noncompliant
        Trace.WriteLineIf(_traceSwitch.TraceError, 42);                     // Noncompliant
        Trace.WriteLineIf(_traceSwitch.TraceError, "Message", "Category");  // Noncompliant
        Trace.WriteLineIf(_traceSwitch.TraceError, 42, "Category");         // Noncompliant
    }

    public void TraceSwitch_Properties(bool condition)
    {
        Trace.WriteIf(_traceSwitch.TraceError, "Message");                  // Noncompliant
        Trace.WriteIf(_traceSwitch.TraceInfo, "Message");                   // Noncompliant
        Trace.WriteIf(_traceSwitch.TraceVerbose, "Message");                // Noncompliant
        Trace.WriteIf(_traceSwitch.TraceWarning, "Message");                // Noncompliant
        Trace.WriteIf(_traceSwitch.Level == TraceLevel.Error, "Message");   // Noncompliant
        Trace.WriteIf(_traceSwitch.Level != TraceLevel.Off, "Message");     // Noncompliant
    }

    public void Not_TraceClass(string arg)
    {
        Debug.WriteIf(_traceSwitch.TraceError, "Message");                  // Compliant - the method is not from the System.Diagnostics.Trace class
        Debug.WriteIf(_traceSwitch.TraceError, "Message: {0}", arg);
        Debug.WriteLineIf(_traceSwitch.TraceError, "Message");
        Debug.WriteLineIf(_traceSwitch.TraceError, "Message: {0}", arg);
    }

    public void Aliased_Trace(AliasedTraceSwitch aliasedTraceSwitch)
    {
        AliasedTrace.WriteIf(aliasedTraceSwitch.TraceError, "Message");     // Noncompliant
        AliasedTrace.WriteLineIf(aliasedTraceSwitch.TraceError, "Message"); // Noncompliant
    }

    public void Custom_TraceSwitch(CustomTraceSwitch customTraceSwitch)
    {
        AliasedTrace.WriteIf(customTraceSwitch.TraceError, "Message");      // Noncompliant
        AliasedTrace.WriteLineIf(customTraceSwitch.TraceError, "Message");  // Noncompliant
    }

    public class CustomTraceSwitch : TraceSwitch
    {
        public CustomTraceSwitch(string displayName, string description) : base(displayName, description)
        {
        }
    }
}

namespace MyNamespace
{
    public class Test
    {
        private System.Diagnostics.TraceSwitch _regularTraceSwitch;
        private TraceSwitch _fakeTraceSwitch;

        public void Using_CustomClass(string arg)
        {
            Trace.WriteIf(_regularTraceSwitch.TraceError, "Message");                   // Compliant - the method is not from the System.Diagnostics.Trace class
            Trace.WriteIf(_regularTraceSwitch.TraceError, "Message: {0}", arg);
            Trace.WriteLineIf(_regularTraceSwitch.TraceError, "Message");
            Trace.WriteLineIf(_regularTraceSwitch.TraceError, "Message: {0}", arg);

            System.Diagnostics.Trace.WriteIf(_fakeTraceSwitch.TraceError, "Message");   // Compliant - the TraceSwitch object in the if statement is not an instance of the System.Diagnostics.TraceSwitch class
            System.Diagnostics.Trace.WriteIf(_fakeTraceSwitch.TraceError, "Message: {0}", arg);
            System.Diagnostics.Trace.WriteLineIf(_fakeTraceSwitch.TraceError, "Message");
            System.Diagnostics.Trace.WriteLineIf(_fakeTraceSwitch.TraceError, "Message: {0}", arg);
        }
    }

    public static class Trace
    {
        public static void WriteIf(bool condition, string message) { }
        public static void WriteIf(bool condition, string message, params object[] args) { }
        public static void WriteLineIf(bool condition, string message) { }
        public static void WriteLineIf(bool condition, string message, params object[] args) { }
    }

    public class TraceSwitch
    {
        public bool TraceError { get; }
    }
}
