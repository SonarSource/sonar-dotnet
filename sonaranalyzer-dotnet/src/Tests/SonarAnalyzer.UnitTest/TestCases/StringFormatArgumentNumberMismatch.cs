﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests.Diagnostics
{
    public class StringFormatArgumentNumberMismatch
    {
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
            s = string.Format("no format"); // Compliant

            s = string.Format("{0}", arg0, arg1); // Noncompliant {{Invalid string format, the following arguments are unused: 'arg1'.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            s = string.Format("{0}", arg0, arg1, arg2); // Noncompliant {{Invalid string format, the following arguments are unused: 'arg1', 'arg2'.}}

            s = string.Format("[0}", arg0); // Noncompliant {{Invalid string format, unbalanced curly brace count.}}
            s = string.Format("{{0}", arg0); // Noncompliant
            s = string.Format("{0}}", arg0); // Noncompliant

            s = string.Format("{-1}", arg0); // Noncompliant {{Invalid string format, opening curly brace can only be followed by a digit or an opening curly brace.}}
            s = string.Format(null, "{}"); // Noncompliant

            s = string.Format("{0} {1}", arg0); // Noncompliant {{Invalid string format, the highest string format item index should not be greater than the arguments count.}}
            s = string.Format("{0} {1} {2}", new[] { 1, 2 }); // Noncompliant
            s = string.Format("{0} {1} {2}", new object[] { 1, 2 }); // Noncompliant

            s = string.Format("{0} {2}", arg0, arg1); // Noncompliant {{Invalid string format, the following format item indexes are missing: '1'.}}
            s = string.Format("{0} {2}", arg0, arg1, arg2); // Noncompliant
            s = string.Format(null, "{2000}"); // Noncompliant

            var pattern = "{0} {1} {2}";
            s = string.Format(pattern, 1, 2); // Compliant, not const string are not recognized

            int[] intArray = new int[] { };
            s = string.Format("{0} {1} {2}", intArray); // Compliant, arrays are not recognized
            s = string.Format("{0} {1} {2}", args); // Compliant, arrays are not recognized
            s = string.Format("{0}", intArray, intArray); // Noncompliant

            const string pattern2 = "{0} {1} {2}";
            s = string.Format(pattern2, 1, 2); // Noncompliant
            s = string.Format(null, pattern2, 1, 2); // Noncompliant
            s = string.Format(null, pattern2, 1, 2, 3); // Compliant

            s = string.Format(null, arg0: 1); // Noncompliant {{Invalid string format, the format cannot be null.}}

            s = string.Format("{0:C,10}", 42); // Noncompliant {{Invalid string format, format items should comply with the following pattern '{index[,alignment][:formatString]}'.}}

            s = string.Format("{0test}", 42); // Noncompliant {{Invalid string format, format item index should be a number.}}

            s = string.Format("{0,test}", 42); // Noncompliant {{Invalid string format, format item alignment should be a number.}}
        }
        void System_Console_Write(string[] args)
        {
            Console.Write("0");
            Console.Write("{0}", 42);

            Console.Write("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Console.Write("[0}", args[0]); // Noncompliant
            Console.Write("{-1}", args[0]); // Noncompliant
            Console.Write("{0} {1}", args[0]); // Noncompliant
            Console.Write("{2}", 1, 2, 3); // Noncompliant
            Console.Write(null, 1, 2); // Noncompliant
        }

        void System_Console_WriteLine(string[] args)
        {
            Console.WriteLine("0");
            Console.WriteLine("{0}", 42);

            Console.WriteLine("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Console.WriteLine("[0}", args[0]); // Noncompliant
            Console.WriteLine("{-1}", args[0]); // Noncompliant
            Console.WriteLine("{0} {1}", args[0]); // Noncompliant
            Console.WriteLine("{2}", 1, 2, 3); // Noncompliant
            Console.WriteLine(null, 1, 2); // Noncompliant
        }

        void System_Text_StringBuilder_AppendFormat(string[] args)
        {
            System.Text.StringBuilder.AppendFormat("0");
            System.Text.StringBuilder.AppendFormat("{0}", 42);

            System.Text.StringBuilder.AppendFormat("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Text.StringBuilder.AppendFormat("[0}", args[0]); // Noncompliant
            System.Text.StringBuilder.AppendFormat("{-1}", args[0]); // Noncompliant
            System.Text.StringBuilder.AppendFormat("{0} {1}", args[0]); // Noncompliant
            System.Text.StringBuilder.AppendFormat("{2}", 1, 2, 3); // Noncompliant
            System.Text.StringBuilder.AppendFormat(null, 1, 2); // Noncompliant
        }

        void System_IO_TextWriter_Write(string[] args)
        {
            System.IO.TextWriter.Write("0");
            System.IO.TextWriter.Write("{0}", 42);

            System.IO.TextWriter.Write("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.IO.TextWriter.Write("[0}", args[0]); // Noncompliant
            System.IO.TextWriter.Write("{-1}", args[0]); // Noncompliant
            System.IO.TextWriter.Write("{0} {1}", args[0]); // Noncompliant
            System.IO.TextWriter.Write("{2}", 1, 2, 3); // Noncompliant
            System.IO.TextWriter.Write(null, 1, 2); // Noncompliant
        }

        void System_IO_TextWriter_WriteLine(string[] args)
        {
            System.IO.TextWriter.WriteLine("0");
            System.IO.TextWriter.WriteLine("{0}", 42);

            System.IO.TextWriter.WriteLine("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.IO.TextWriter.WriteLine("[0}", args[0]); // Noncompliant
            System.IO.TextWriter.WriteLine("{-1}", args[0]); // Noncompliant
            System.IO.TextWriter.WriteLine("{0} {1}", args[0]); // Noncompliant
            System.IO.TextWriter.WriteLine("{2}", 1, 2, 3); // Noncompliant
            System.IO.TextWriter.WriteLine(null, 1, 2); // Noncompliant
        }

        void System_Diagnostics_Debug_WriteLine(string[] args)
        {
            System.Diagnostics.Debug.WriteLine("0");
            System.Diagnostics.Debug.WriteLine("{0}", 42);

            System.Diagnostics.Debug.WriteLine("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Debug.WriteLine("[0}", args[0]); // Noncompliant
            System.Diagnostics.Debug.WriteLine("{-1}", args[0]); // Noncompliant
            System.Diagnostics.Debug.WriteLine("{0} {1}", args[0]); // Noncompliant
            System.Diagnostics.Debug.WriteLine("{2}", 1, 2, 3); // Noncompliant
            System.Diagnostics.Debug.WriteLine(null, 1, 2); // Noncompliant
        }

        void System_Diagnostics_Trace_TraceError(string[] args)
        {
            System.Diagnostics.Trace.TraceError("0");
            System.Diagnostics.Trace.TraceError("{0}", 42);

            System.Diagnostics.Trace.TraceError("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Trace.TraceError("[0}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceError("{-1}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceError("{0} {1}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceError("{2}", 1, 2, 3); // Noncompliant
            System.Diagnostics.Trace.TraceError(null, 1, 2); // Noncompliant
        }

        void System_Diagnostics_Trace_TraceInformation(string[] args)
        {
            System.Diagnostics.Trace.TraceInformation("0");
            System.Diagnostics.Trace.TraceInformation("{0}", 42);

            System.Diagnostics.Trace.TraceInformation("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Trace.TraceInformation("[0}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceInformation("{-1}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceInformation("{0} {1}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceInformation("{2}", 1, 2, 3); // Noncompliant
            System.Diagnostics.Trace.TraceInformation(null, 1, 2); // Noncompliant
        }

        void System_Diagnostics_Trace_TraceWarning(string[] args)
        {
            System.Diagnostics.Trace.TraceWarning("0");
            System.Diagnostics.Trace.TraceWarning("{0}", 42);

            System.Diagnostics.Trace.TraceWarning("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.Trace.TraceWarning("[0}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceWarning("{-1}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceWarning("{0} {1}", args[0]); // Noncompliant
            System.Diagnostics.Trace.TraceWarning("{2}", 1, 2, 3); // Noncompliant
            System.Diagnostics.Trace.TraceWarning(null, 1, 2); // Noncompliant
        }

        void System_Diagnostics_TraceSource_TraceInformation(string[] args)
        {
            System.Diagnostics.TraceSource.TraceInformation("0");
            System.Diagnostics.TraceSource.TraceInformation("{0}", 42);

            System.Diagnostics.TraceSource.TraceInformation("{0}", args[0], args[1]); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            System.Diagnostics.TraceSource.TraceInformation("[0}", args[0]); // Noncompliant
            System.Diagnostics.TraceSource.TraceInformation("{-1}", args[0]); // Noncompliant
            System.Diagnostics.TraceSource.TraceInformation("{0} {1}", args[0]); // Noncompliant
            System.Diagnostics.TraceSource.TraceInformation("{2}", 1, 2, 3); // Noncompliant
            System.Diagnostics.TraceSource.TraceInformation(null, 1, 2); // Noncompliant
        }
    }
}