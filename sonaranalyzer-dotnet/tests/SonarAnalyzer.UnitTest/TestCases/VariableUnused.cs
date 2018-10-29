using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tests.Diagnostics
{
    public class VariableUnused
    {
        void F1()
        {
            var packageA = DoSomething("Foo", "1.0");
            var packageB = DoSomething("Qux", "1.0");

            var localRepository = new Cl { packageA, packageB }; // Noncompliant
//              ^^^^^^^^^^^^^^^

            using (var x = new StreamReader("")) // Compliant
            {
                var v = 5; // Noncompliant {{Remove the unused local variable 'v'.}}
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

    public class Lambdas
    {
        public void Foo(List<int> list)
        {
            list.Select(item => 1);

            list.Select(item =>
                {
                    var value = 1; // Noncompliant
                    return item;
                });
        }
    }
}
