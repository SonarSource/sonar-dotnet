﻿using System;
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

            Action notUsed = () => // Noncompliant
            {
                Console.WriteLine("Hello world.");
            };

            _ = 3;

            string c;
            c = "Hello, world!";
            Console.WriteLine(c);

            string f1 = "Hello"; // Noncompliant

            var d = "";
            var e = new List<String> { d };
            Console.WriteLine(e);

            string f;
            f = "something"; // Compliant, S1854 (Deadstores) reports on this.
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

    public class WithLocalFunctions
    {
        public void Method()
        {
            void Foo(List<int> list)
            {
                list.Select(item => 1);

                list.Select(item =>
                {
                    var value = 1; // Noncompliant
                    return item;
                });
            }

            static void Bar(List<int> list)
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

    //https://github.com/SonarSource/sonar-dotnet/issues/3137
    public class Repro_3137
    {
        public void GoGoGo(Logger log)
        {
            using var scope = log.BeginScope("Abc"); // Compliant, existence of variable represents a state until it's disposed
            using var _ = log.BeginScope("XXX"); // Underscore is a variable in this case, it's not a discard pattern

            // Locked file represents a state until it's disposed
            using var stream = File.Create("path");
        }

        public class Logger
        {
            public IDisposable BeginScope(string scope)
            {
                return null;
            }
        }
    }

    public class Tuples
    {
        public static (int foo, int bar) M(string text)
        {
            int b = int.Parse(text);
            return (1, b);
        }

        public void UnusedTuple()
        {
            (int x, int y) t = (1, 2); // Noncompliant
        }

        public void UsedTuple()
        {
            (int x, int y) t = (1, 2);
            Use(t);
        }

        private void Use((int, int) t) { }
    }
}
