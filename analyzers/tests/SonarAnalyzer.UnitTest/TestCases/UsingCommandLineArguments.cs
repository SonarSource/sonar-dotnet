using System;

namespace Tests.Diagnostics
{
    class Program1
    {
        public static void Main(params string[] args) // Noncompliant {{Make sure that command line arguments are used safely here.}}
//                         ^^^^
        {
            Console.WriteLine(args[0]);
        }
    }

    class Program2
    {
        public static void Main(params string[] args) // Compliant, args is not used
        {
        }

        public static void Main(string arg) // Compliant, not a Main method
        {
            Console.WriteLine(arg);
        }

        public static void Main(int x, params string[] args) // Compliant, not a Main method
        {
            Console.WriteLine(args);
        }
    }

    class Program3
    {
        private static string[] args;
        public static void Main(params string[] args) // Compliant, args is not used
        {
            Console.WriteLine(Program3.args);
        }
    }

    class Program4
    {
        public static string Main(params string[] args) // Compliant, not a Main method
        {
            Console.WriteLine(args);
            return null;
        }
    }
}
