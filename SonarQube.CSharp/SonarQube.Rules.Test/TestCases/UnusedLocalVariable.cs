using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class UnusedLocalVariable2
    {
        void F1()
        {
            var packageA = DoSomething("Foo", "1.0");
            var packageB = DoSomething("Qux", "1.0");

            var localRepository = new Cl { packageA, packageB }; // Noncompliant

            using (var x = new StreamReader("")) // Noncompliant
            {
                var v = 5; // Noncompliant
            }

            int a; // Noncompliant
            var b = (Action<int>)(
                _ =>
                {
                    int i; // Noncompliant
                    int j = 42;
                    Console.WriteLine("Hello, world!" + j);
                });

            b(5);

            string c;
            c = "Hello, world!";
            Console.WriteLine(c);

            var d = "";
            var e = new List<String> { d };
            Console.WriteLine(e);
        }

        private object DoSomething(string foo, string p1)
        {
            throw new NotImplementedException();
        }

        void F2(int a)
        {
        }
    }

    internal class Cl : List<object>
    {
    }
}
