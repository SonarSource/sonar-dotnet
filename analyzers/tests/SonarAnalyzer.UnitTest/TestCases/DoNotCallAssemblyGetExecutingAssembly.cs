using System;
using System.Reflection;

namespace Tests.Diagnostics
{
    class Program
    {
        public static void Main()
        {
            Assembly assem = Assembly.GetExecutingAssembly(); // Noncompliant
            Console.WriteLine("Assembly name: {0}", assem.FullName);

            assem = typeof(Program).Assembly; // Compliant
            Console.WriteLine("Assembly name: {0}", assem.FullName);
        }
    }
}
