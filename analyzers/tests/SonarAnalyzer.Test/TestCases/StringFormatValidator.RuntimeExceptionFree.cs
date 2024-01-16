using System;

public class StringFormatValidatorRuntimeExceptionFree
{
    private string validFormatField = "{0}";
    private string invalidFormatField = "{{0}";

    void System_String_Format(string[] args)
    {
        string s;
        string arg0 = string.Empty;
        string arg1 = string.Empty;
        string arg2 = string.Empty;

        s = string.Format("{0}", 42); // Compliant
        s = string.Format("{0,10}", 42); // Compliant
        s = string.Format("{0,-10}", 42); // Compliant
        s = string.Format("{0:0000}", 42); // Compliant
        s = string.Format("{2}-{0}-{1}", 1, 2, 3); // Compliant
        s = string.Format("{{{0}}}", 42); // Compliant, displays {42}
        s = string.Format("{{\r\n{0}}}", 42); // Compliant, displays {\r\n42}
        s = string.Format("{0, -12}", 42);
        s = string.Format("{0    ,    -12}", 42);
        s = string.Format("{0   ,   -20    :N1}", 1.234);

        s = string.Format("[0}", arg0); // Noncompliant {{Invalid string format, unbalanced curly brace count.}}
        //  ^^^^^^^^^^^^^
        s = string.Format("{{0}", arg0); // Noncompliant
        s = string.Format("{0}}", arg0); // Noncompliant

        s = string.Format("{-1}", arg0); // Noncompliant {{Invalid string format, opening curly brace can only be followed by a digit or an opening curly brace.}}
        s = string.Format(null, "{}"); // Noncompliant
        s = string.Format("}{ {0}", 42); // Noncompliant

        s = string.Format("{0} {1}", arg0); // Noncompliant {{Invalid string format, the highest string format item index should not be greater than the arguments count.}}
        s = string.Format("{0} {1} {2}", new[] { 1, 2 }); // Noncompliant
        s = string.Format("{0} {1} {2}", new object[] { 1, 2 }); // Noncompliant

        var pattern = "{0} {1} {2}";
        s = string.Format(pattern, 1, 2); // Noncompliant

        int[] intArray = new int[] { };
        s = string.Format("{0} {1} {2}", intArray); // Compliant, arrays are not recognized
        s = string.Format("{0} {1} {2}", args); // Compliant, arrays are not recognized

        const string pattern2 = "{0} {1} {2}";
        s = string.Format(pattern2, 1, 2); // Noncompliant
        s = string.Format(null, pattern2, 1, 2); // Noncompliant
        s = string.Format(null, pattern2, 1, 2, 3); // Compliant

        s = string.Format(null, arg0: 1); // Noncompliant {{Invalid string format, the format string cannot be null.}}

        s = string.Format("{0test}", 42); // Noncompliant {{Invalid string format, all format items should comply with the following pattern '{index[,alignment][:formatString]}'.}}
        s = string.Format("{0,test}", 42); // Noncompliant
        s = string.Format("{0.0}", 42); // Noncompliant
        s = string.Format("{0:C,10}", 42);
        s = string.Format("{0:hh:mm:ss}", DateTime.Now);
        s = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
        s = string.Format("{0:(###) ###-####}", 8005551212);
        s = string.Format("{0:(###) ###-####}", 8005551212);
        s = string.Format("{0:0,.}", 1500.42);
        s = string.Format("{0:0,0}", 1500.42);

        s = string.Format("{1000001}"); // Noncompliant {{Invalid string format, the string format item index should not be greater than 1000000.}}
        s = string.Format("{0,1000001}"); // Noncompliant {{Invalid string format, the string format item alignment should not be greater than 1000000.}}

        var validFormatVariable = "{0}";
        var invalidFormatVariable = "{{0}";
        s = string.Format(invalidFormatField, arg0);    // Noncompliant
        s = string.Format(invalidFormatVariable, arg0); // Noncompliant
        s = string.Format(validFormatField, arg0);
        s = string.Format(validFormatVariable, arg0);
        // Reassigned to fixed values
        invalidFormatField = "{0}";
        invalidFormatVariable = "{0}";
        s = string.Format(invalidFormatField, arg0);
        s = string.Format(invalidFormatVariable, arg0);
    }

    void System_Console_Write(string[] args)
    {
        Console.Write("0");
        Console.Write("{0}", 42);
        Console.Write("{{}}"); // Compliant, displays {}
        Console.Write("{"); // Compliant
        Console.Write(ulong.MaxValue);

        Console.Write("[0}", args[0]); // Noncompliant
        Console.Write("{-1}", args[0]); // Noncompliant
        Console.Write("{0} {1}", args[0]); // Noncompliant
        Console.Write(null, "foo", "bar"); // Noncompliant
    }

    void System_Console_WriteLine(string[] args)
    {
        Console.WriteLine(null, "foo", "bar"); // Noncompliant
    }

    void System_Text_StringBuilder_AppendFormat(string[] args)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendFormat("{0}", 42);

        sb.AppendFormat("[0}", args[0]); // Noncompliant
        sb.AppendFormat("{-1}", args[0]); // Noncompliant
        sb.AppendFormat("{0} {1}", args[0]); // Noncompliant
        sb.AppendFormat(null, "foo", "bar"); // Noncompliant
    }

    void System_IO_TextWriter_Write(string[] args)
    {
        System.IO.TextWriter tw = new System.IO.StreamWriter("");

        tw.Write("0");
        tw.Write("{0}", 42);
        tw.Write("{"); // Compliant
        tw.Write(ulong.MaxValue);

        tw.Write("[0}", args[0]); // Noncompliant
        tw.Write("{-1}", args[0]); // Noncompliant
        tw.Write("{0} {1}", args[0]); // Noncompliant
        tw.Write(null, "foo", "bar"); // Noncompliant
    }

    void System_IO_TextWriter_WriteLine(string[] args)
    {
        System.IO.TextWriter tw = new System.IO.StreamWriter("");

        tw.WriteLine("0");
        tw.WriteLine("{0}", 42);
        tw.WriteLine("{"); // Compliant
        tw.WriteLine(ulong.MaxValue);

        tw.WriteLine("[0}", args[0]); // Noncompliant
        tw.WriteLine("{-1}", args[0]); // Noncompliant
        tw.WriteLine("{0} {1}", args[0]); // Noncompliant
        tw.WriteLine(null, "foo", "bar"); // Noncompliant
    }

    void System_Diagnostics_Debug_WriteLine(string[] args)
    {
        System.Diagnostics.Debug.WriteLine("0");
        System.Diagnostics.Debug.WriteLine("{0}", 42);
        System.Diagnostics.Debug.WriteLine("{"); // Compliant
        System.Diagnostics.Debug.WriteLine(ulong.MaxValue);
        System.Diagnostics.Debug.WriteLine("", "");

        System.Diagnostics.Debug.WriteLine("[0}", args); // Noncompliant
        System.Diagnostics.Debug.WriteLine("{-1}", args); // Noncompliant
        System.Diagnostics.Debug.WriteLine("{0} {1}", (object)args[0]); // Noncompliant
        System.Diagnostics.Debug.WriteLine(null, "foo", "bar"); // Noncompliant
    }

    void System_Diagnostics_Trace_TraceError(string[] args)
    {
        System.Diagnostics.Trace.TraceError("0");
        System.Diagnostics.Trace.TraceError("{0}", 42);
        System.Diagnostics.Trace.TraceError("{"); // Compliant

        System.Diagnostics.Trace.TraceError("[0}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceError("{-1}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceError("{0} {1}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceError(null, "foo", "bar"); // Noncompliant
    }

    void System_Diagnostics_Trace_TraceInformation(string[] args)
    {
        System.Diagnostics.Trace.TraceInformation("0");
        System.Diagnostics.Trace.TraceInformation("{0}", 42);
        System.Diagnostics.Trace.TraceInformation("{"); // Compliant

        System.Diagnostics.Trace.TraceInformation("[0}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceInformation("{-1}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceInformation("{0} {1}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceInformation(null, "foo", "bar"); // Noncompliant
    }

    void System_Diagnostics_Trace_TraceWarning(string[] args)
    {
        System.Diagnostics.Trace.TraceWarning("0");
        System.Diagnostics.Trace.TraceWarning("{0}", 42);
        System.Diagnostics.Trace.TraceWarning("{"); // Compliant

        System.Diagnostics.Trace.TraceWarning("[0}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceWarning("{-1}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceWarning("{0} {1}", args[0]); // Noncompliant
        System.Diagnostics.Trace.TraceWarning(null, "foo", "bar"); // Noncompliant
    }

    void System_Diagnostics_TraceSource_TraceInformation(string[] args)
    {
        var ts = new System.Diagnostics.TraceSource("");

        ts.TraceInformation("0");
        ts.TraceInformation("{0}", 42);
        ts.TraceInformation("{"); // Compliant

        ts.TraceInformation("[0}", args[0]); // Noncompliant
        ts.TraceInformation("{-1}", args[0]); // Noncompliant
        ts.TraceInformation("{0} {1}", args[0]); // Noncompliant
        ts.TraceInformation(null, "foo", "bar"); // Noncompliant
    }
}
