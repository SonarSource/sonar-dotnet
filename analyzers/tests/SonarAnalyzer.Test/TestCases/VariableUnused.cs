using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using A;
using B;

namespace Tests.Diagnostics
{
    public class VariableUnused
    {
        void LinqRangeVariables()
        {
            var byteArray = new byte[10];

            _ = from a in byteArray //Compliant
                let b = a / 2
                let c = b + a
                group c by c * b into g
                join d in new object[0] on g equals d into f
                select f into z
                select z;

            _ = from a in byteArray // Compliant
                let b = a
                let c = 1
                group c by b into d
                where d != null
                let y = 2
                let z = 1
                orderby y
                select z;

            _ = from a in byteArray // Compliant
                let b = a
                let c = 1
                let d = 8 * 12
                let e = -13
                select a * b + c - d / e;

            _ = from a in byteArray                             //Noncompliant
//                   ^
                let b = true                                    //Noncompliant
//                  ^
                let c = false
                group c by c into g
                join d in new object[0] on g equals d into f    //Noncompliant
//                                                         ^
                select g into e                                 //Noncompliant
//                            ^
                select true;

            var byteArrayB = new byte[10];

            _ = from upper in byteArray
                from lower in byteArrayB
                let a = lower - 10
                let b = upper - 10
                let c = -10                     //Noncompliant
                where upper != lower && lower > b
                orderby a descending
                select upper;

            _ = from a in byteArray //Noncompliant
                let b = 0
                select b;

            _ = from a in byteArray
                let b = 0
                where a != 0
                select b;

            _ = from a in byteArray
                let b = 0
                let c = 1   //Noncompliant
                where a != 0
                select b;

            _ = from a in byteArray //Noncompliant
                let b = 0
                where b != 0
                select b;

            _ = from a in "HELLO"   //Noncompliant
                let c = "a"
                let b = c   //Noncompliant
                let z = ""
                where z is null
                let x = false
                group x by x into w //Noncompliant
                let p = "p"
                orderby p descending
                select p;

            //Nested subquery
            _ = from a in "HELLO"
                let b = "a"
                group a by b into c
                select new
                {
                    Key = c.Key,
                    Value =
                        from z in c //Noncompliant
                        select "1"
                };
            _ = from _ in byteArray // Compliant, don't report on discard like variables
                select 1;
        }

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
            var _ = 1; // Compliant, don't report on discard like variables
        }

        void Bar(out string a)
        {
            a = "1";
        }

        void F3()
        {
            var (_, b) = (1, 2); // Noncompliant
            //      ^

            Bar(out _);
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

    public class CSharpSevenVariableDeclarations
    {
        public void Foo()
        {

            var x = "hello";
            if (x is string s) // Noncompliant
//                          ^
            {
                x += ":)";
            }

            if (x is string t) // Compliant
            {
               t += ":)";
            }

            Bar(out var b); // Noncompliant
//                      ^
            Bar(out var c); //Compliant
            c++;

            void Bar(out int num)
            {
                num = 42;
            }
            ref string refX = ref x; //Noncompliant
//                     ^^^^
            ref string refXX = ref x; //Compliant
            refXX += "1";

            var tuple = ("hello", "bye");
            if (tuple is (string strX, string strY)) // Noncompliant
//                                            ^^^^
            {
                strX += ":)";
            }

            if (tuple is (string comX, string comY)) // Compliant
            {
                comX += comY;
            }

            if (x is string { Length:5 } rec) //Noncompliant
//                                       ^^^
            {
                x += ":)";
            }
            if (x is string { Length: 5 } com) //Compliant
            {
                com += ":)";
            }

            if (tuple is (string str, { } d)) // Noncompliant
//                                        ^
            {
                str += ":)";
            }

            if (tuple is (string y, { } z)) // Noncompliant
//                               ^
            {
                z += ":)";
            }

            if (tuple is (string strCom, { } dCom)) // Compliant
            {
                strCom += dCom;
            }
        }

        public void SwitchStatements()
        {
            var o = new Shape();

            switch (o)
            {
                case Circle c: //Noncompliant
//                          ^
                    break;
                case Square s: //Compliant
                    s.side = 10;
                    break;
                default:
                    break;
            }
        }
        public class Shape
        {
            public int A;
        }

        public class Square : Shape
        {
            public int side = 0;
        }

        public class Circle : Shape
        {
            public int radius = 0;
        }
    }

    public class Loops
    {
        public void ForEach()
        {
            foreach (var unused in new int[] { 1, 2, 3 }) // Compliant
            { }
            foreach (var _ in new int[] { 1, 2, 3 })      // Compliant
            { }
        }

        public void For()
        {
            for (var i = 0; ;) // Compliant
            { }
            for (var _ = 0; ;) // Compliant
            { }
        }
    }
}

public class TestWithAmbiguous
{

    public void SomeMethod()
    {
        var a = new Ambiguous();    // Noncompliant
                                    // Error@-1 [CS0104]
    }
}

namespace A
{
    public class Ambiguous
    { }
}

namespace B
{
    public class Ambiguous
    { }
}
