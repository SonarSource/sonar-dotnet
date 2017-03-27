using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Tests.Diagnostics
{
    public class StringFormatTypoFreeValidator
    {
        void Test()
        {
            string arg0 = string.Empty;
            string arg1 = string.Empty;
            string arg2 = string.Empty;

            var s = string.Format("some text"); // Noncompliant {{Remove this formatting call and simply use the input string.}}
//                  ^^^^^^^^^^^^^
            s = string.Format(string.Format("foo"));    //Noncompliant
            s = string.Format(string.Format("{0}", 42));

            s = string.Format("{0}", 1);
            s = string.Format("{0}");
            s = string.Format(CultureInfo.InvariantCulture, "{0}", 3);
            s = string.Format(CultureInfo.InvariantCulture, "some text"); //Noncompliant
            s = string.Format(format: "some text"); //Noncompliant
            s = string.Format(format: "{0}", arg0: 1);

            s = string.Format("{0}", arg0, arg1); // Noncompliant {{The format string might be wrong, the following arguments are unused: 'arg1'.}}
            s = string.Format("{0}", arg0, arg1, arg2); // Noncompliant {{The format string might be wrong, the following arguments are unused: 'arg1', 'arg2'.}}

            s = string.Format("{0} {2}", arg0, arg1);
            s = string.Format("{0} {2}", arg0, arg1, arg2); // Noncompliant {{The format string might be wrong, the following item indexes are missing: '1'.}}
        }

        void System_Console_Write(string[] args)
        {
            Console.Write("0"); // Compliant
            Console.Write("{0}", 42);
            Console.Write("0", 25); // Noncompliant

            Console.Write("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^
            Console.Write("{2}", 1, 2, 3); // Noncompliant
        }

        void System_Console_WriteLine(string[] args)
        {
            Console.WriteLine("0"); // Compliant
            Console.WriteLine("{0}", 42);

            Console.WriteLine("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^
            Console.WriteLine("{2}", 1, 2, 3); // Noncompliant
        }

        void System_Text_StringBuilder_AppendFormat(string[] args)
        {
            System.Text.StringBuilder.AppendFormat("0"); // Noncompliant
            System.Text.StringBuilder.AppendFormat("{0}", 42);

            System.Text.StringBuilder.AppendFormat("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Text.StringBuilder.AppendFormat("{2}", 1, 2, 3); // Noncompliant
        }

        void System_IO_TextWriter_Write(string[] args)
        {
            System.IO.TextWriter.Write("0"); // Compliant
            System.IO.TextWriter.Write("{0}", 42);

            System.IO.TextWriter.Write("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.IO.TextWriter.Write("{2}", 1, 2, 3); // Noncompliant
        }

        void System_IO_TextWriter_WriteLine(string[] args)
        {
            System.IO.TextWriter.WriteLine("0"); // Compliant
            System.IO.TextWriter.WriteLine("{0}", 42);

            System.IO.TextWriter.WriteLine("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.IO.TextWriter.WriteLine("{2}", 1, 2, 3); // Noncompliant
        }

        void System_Diagnostics_Debug_WriteLine(string[] args)
        {
            System.Diagnostics.Debug.WriteLine("0"); // Compliant
            System.Diagnostics.Debug.WriteLine("{0}", 42);

            System.Diagnostics.Debug.WriteLine("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Debug.WriteLine("{2}", 1, 2, 3); // Noncompliant
        }

        void System_Diagnostics_Trace_TraceError(string[] args)
        {
            System.Diagnostics.Trace.TraceError("0"); // Compliant
            System.Diagnostics.Trace.TraceError("{0}", 42);

            System.Diagnostics.Trace.TraceError("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Trace.TraceError("{2}", 1, 2, 3); // Noncompliant
        }

        void System_Diagnostics_Trace_TraceInformation(string[] args)
        {
            System.Diagnostics.Trace.TraceInformation("0"); // Compliant
            System.Diagnostics.Trace.TraceInformation("{0}", 42);

            System.Diagnostics.Trace.TraceInformation("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Trace.TraceInformation("{2}", 1, 2, 3); // Noncompliant
        }

        void System_Diagnostics_Trace_TraceWarning(string[] args)
        {
            System.Diagnostics.Trace.TraceWarning("0"); // Compliant
            System.Diagnostics.Trace.TraceWarning("{0}", 42);

            System.Diagnostics.Trace.TraceWarning("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Trace.TraceWarning("{2}", 1, 2, 3); // Noncompliant
        }

        void System_Diagnostics_TraceSource_TraceInformation(string[] args)
        {
            System.Diagnostics.TraceSource.TraceInformation("0"); // Compliant
            System.Diagnostics.TraceSource.TraceInformation("{0}", 42);

            System.Diagnostics.TraceSource.TraceInformation("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.TraceSource.TraceInformation("{2}", 1, 2, 3); // Noncompliant
        }
    }
}
