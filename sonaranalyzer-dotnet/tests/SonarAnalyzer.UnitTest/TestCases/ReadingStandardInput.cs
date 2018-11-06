using System;
using Con = System.Console;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method()
        {
            string value;
            int code;
            ConsoleKeyInfo key;

            code = System.Console.Read(); // Noncompliant {{Make sure that reading the standard input is safe here.}}
//                 ^^^^^^^^^^^^^^^^^^^^^
            code = Con.Read(); // Noncompliant

            value = Console.ReadLine(); // Noncompliant
            code = Console.Read(); // Noncompliant
            key = Console.ReadKey(); // Noncompliant
            key = Console.ReadKey(true); // Noncompliant

            Console.Read(); // Compliant, value is ignored
            Console.ReadLine(); // Compliant, value is ignored
            Console.ReadKey(); // Compliant, value is ignored
            Console.ReadKey(true); // Compliant, value is ignored

            Console.OpenStandardInput(); // Noncompliant
            Console.OpenStandardInput(100); // Noncompliant

            var x = System.Console.In; // Noncompliant
//                  ^^^^^^^^^^^^^^^^^
            x = Console.In; // Noncompliant
            x = Con.In; // Noncompliant
            Console.In.Read(); // Noncompliant

            // Other Console methods
            Console.Write(1);
            Console.WriteLine(1);
            // Other classes
            MyConsole.Read();
            MyConsole.In.Read();
        }
    }

    public static class MyConsole
    {
        public static int Read() => 1;
        public static int ReadKey() => 1;
        public static System.IO.TextReader In { get; }
    }
}
