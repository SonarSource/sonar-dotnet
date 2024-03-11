using System;
using System.Diagnostics;
using AliasedTrace = System.Diagnostics.Trace;

public class Program
{
    public void Noncompliant_TraceMethods()
    {
        Trace.Write("Message");             // Noncompliant {{Avoid using Trace.Write, use instead methods that specify the trace event type.}}
//            ^^^^^
        Trace.WriteLine("Message");         // Noncompliant {{Avoid using Trace.WriteLine, use instead methods that specify the trace event type.}}
    }

    public void Compliant_TraceMethods(string arg)
    {
        Trace.WriteIf(true, "Message");
        Trace.WriteLineIf(true, "Message");

        Trace.TraceError("Message");
        Trace.TraceError("Message: {0}", arg);

        Trace.TraceInformation("Message");
        Trace.TraceInformation("Message: {0}", arg);

        Trace.TraceWarning("Message");
        Trace.TraceWarning("Message: {0}", arg);

        Trace.Write("Message", "Category");             // Compliant - the developer wants to use a specific category, other than the predefined ones
        Trace.Write(42, "Category");

        Trace.WriteLine("Message", "Category");
        Trace.WriteLine(42, "Category");
    }

    public void Write_Overloads()
    {
        Trace.Write("Message");                         // Noncompliant
        Trace.Write(42);                                // Noncompliant

        Trace.WriteLine("Message");                     // Noncompliant
        Trace.WriteLine(42);                            // Noncompliant
    }

    public void Not_TraceClass(string arg)
    {
        Console.Write("Message");
        Console.Write("Message: {0}", arg);
        Console.WriteLine("Message");
        Console.WriteLine("Message: {0}", arg);

        Debug.Write("Message");
        Debug.Write("Message: {0}", arg);
        Debug.WriteLine("Message");
        Debug.WriteLine("Message: {0}", arg);
    }

    public void Aliased_Trace()
    {
        AliasedTrace.Write("Message");                  // Noncompliant
        AliasedTrace.WriteLine("Message");              // Noncompliant
    }
}

namespace MyNamespace
{
    public class Test
    {
        public void Using_CustomTraceClass(string arg)
        {
            Trace.Write("Message");                     // Compliant - the method is not from the System.Diagnostics.Trace class
            Trace.Write("Message: {0}", arg);
            Trace.WriteLine("Message");
            Trace.WriteLine("Message: {0}", arg);
        }
    }

    public static class Trace
    {
        public static void Write(string message) { }
        public static void Write(string message, params object[] args) { }
        public static void WriteLine(string message) { }
        public static void WriteLine(string message, params object[] args) { }
    }
}
