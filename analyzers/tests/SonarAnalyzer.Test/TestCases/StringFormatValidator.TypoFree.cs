using System;
using System.Globalization;

public class StringFormatValidatorTypoFree
{
    void Test()
    {
        string arg0 = string.Empty;
        string arg1 = string.Empty;
        string arg2 = string.Empty;

        var s = string.Format("some text"); // Noncompliant {{Remove this formatting call and simply use the input string.}}
//              ^^^^^^^^^^^^^
        s = string.Format(null, 42);                //Noncompliant {{Invalid string format, the format string cannot be null.}}
        s = string.Format(string.Format("foo"));    //Noncompliant
        s = string.Format(string.Format("{0}", 42));

        s = string.Format("{0}", 1);
        s = string.Format(CultureInfo.InvariantCulture, "{0}", 3);
        s = string.Format(CultureInfo.InvariantCulture, "some text"); //Noncompliant
        s = string.Format(format: "some text"); //Noncompliant
        s = string.Format(format: "{0}", arg0: 1);

        s = string.Format("{0}", arg0, arg1); // Noncompliant {{The format string might be wrong, the following arguments are unused: 'arg1'.}}
        s = string.Format("{0}", arg0, arg1, arg2); // Noncompliant {{The format string might be wrong, the following arguments are unused: 'arg1' and 'arg2'.}}

        s = string.Format("{0} {2}", arg0, arg1, arg2); // Noncompliant {{The format string might be wrong, the following item indexes are missing: '1'.}}
    }

    void System_Console_Write(string[] args)
    {
        Console.Write("0"); // Compliant
        Console.Write("{0}", 42);
        Console.Write("0", 25); // Noncompliant

        Console.Write("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^
        Console.Write("{2}", 1, 2, 3); // Noncompliant
    }

    void System_Console_WriteLine(string[] args)
    {
        Console.WriteLine("0"); // Compliant
        Console.WriteLine("{0}", 42);

        Console.WriteLine("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^^^^^
        Console.WriteLine("{2}", 1, 2, 3); // Noncompliant
    }

    void System_Text_StringBuilder_AppendFormat(string[] args)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendFormat("0"); // Noncompliant
        sb.AppendFormat("{0}", 42);

        sb.AppendFormat("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^^^
        sb.AppendFormat("{2}", 1, 2, 3); // Noncompliant
    }

    void System_IO_TextWriter_Write(string[] args)
    {
        var tw = System.IO.File.CreateText("C:\\perl.txt");
        tw.Write("0"); // Compliant
        tw.Write("{0}", 42);

        tw.Write("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^
        tw.Write("{2}", 1, 2, 3); // Noncompliant
    }

    void System_IO_TextWriter_WriteLine(string[] args)
    {
        var tw = System.IO.File.CreateText("C:\\perl.txt");

        tw.WriteLine("0"); // Compliant
        tw.WriteLine("{0}", 42);

        tw.WriteLine("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^
        tw.WriteLine("{2}", 1, 2, 3); // Noncompliant
    }

    void System_Diagnostics_Debug_WriteLine(string[] args)
    {
        System.Diagnostics.Debug.WriteLine("0"); // Compliant
        System.Diagnostics.Debug.WriteLine("{0}", 42);

        System.Diagnostics.Debug.WriteLine("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        System.Diagnostics.Debug.WriteLine("{2}", 1, 2, 3); // Noncompliant
    }

    void System_Diagnostics_Trace_TraceError(string[] args)
    {
        System.Diagnostics.Trace.TraceError("0"); // Compliant
        System.Diagnostics.Trace.TraceError("{0}", 42);

        System.Diagnostics.Trace.TraceError("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        System.Diagnostics.Trace.TraceError("{2}", 1, 2, 3); // Noncompliant
    }

    void System_Diagnostics_Trace_TraceInformation(string[] args)
    {
        System.Diagnostics.Trace.TraceInformation("0"); // Compliant
        System.Diagnostics.Trace.TraceInformation("{0}", 42);

        System.Diagnostics.Trace.TraceInformation("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        System.Diagnostics.Trace.TraceInformation("{2}", 1, 2, 3); // Noncompliant
    }

    void System_Diagnostics_Trace_TraceWarning(string[] args)
    {
        System.Diagnostics.Trace.TraceWarning("0"); // Compliant
        System.Diagnostics.Trace.TraceWarning("{0}", 42);

        System.Diagnostics.Trace.TraceWarning("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        System.Diagnostics.Trace.TraceWarning("{2}", 1, 2, 3); // Noncompliant
    }

    void System_Diagnostics_TraceSource_TraceInformation(string[] args)
    {
        var ts = new System.Diagnostics.TraceSource("TraceTest");
        ts.TraceInformation("0"); // Compliant
        ts.TraceInformation("{0}", 42);

        ts.TraceInformation("{0}", args[0], args[1]); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^
        ts.TraceInformation("{2}", 1, 2, 3); // Noncompliant
    }
}
