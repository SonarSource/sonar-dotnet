using System;

namespace Tests.Diagnostics
{
    public class UnusedLocalVariable
    {
        void F1()
        {
            int a; // Noncompliant
            var b =
                c =>
                {
                    int i; // Noncompliant
                    int j = 42;
                    Console.WriteLine("Hello, world!" + j);
                }

            b(null);

            string c;
            c = "Hello, world!";
            Console.WriteLine(c);

            var d = "";
            var e = new List<String> { d };
            Console.WriteLine(e);
        }

        void F2(int a)
        {
        }
    }
}
