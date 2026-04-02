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
            // join range variable (JoinClause without 'into') is also tracked
            _ = from a in byteArray
                join b in new byte[0] on a equals (byte)0  // Noncompliant {{Remove the unused local variable 'b'.}}
                select a;

            _ = from a in byteArray
                join b in new byte[0] on a equals (byte)0  // Compliant
                select b;

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

        public void ForeachDeconstruction(IEnumerable<(int, int)> pairs)
        {
            foreach (var (a, b) in pairs) { }       // Noncompliant [issue1, issue2]
            foreach (var (c, d) in pairs)           // Noncompliant
            {
                Console.WriteLine(c);
            }
            foreach (var (_, e) in pairs) { }       // Noncompliant
            foreach (var (f, g) in pairs)           // Compliant
            {
                Console.WriteLine(f + g);
            }
        }
    }

    public class Shadowing
    {
        public void SequentialBlocks()
        {
            { var x = 1; }                                              // Noncompliant - x in second block must not suppress this
            { var x = 2; Console.WriteLine(x); }
        }

        public void TwoLocalFunctions()
        {
            // Both local functions are in the same code block, so both x symbols are collected together.
            // Using x in the second must not suppress the report for x in the first.
            int Fn1() { var x = 1; return 0; }                         // Noncompliant
            int Fn2() { var x = 2; return x; }                         // Compliant
            _ = Fn1() + Fn2();
        }
    }

    public class LocalConstants
    {
        public void Method()
        {
            const int unused = 5;   // Noncompliant
            const int used = 5;
            Console.WriteLine(used);
        }
    }

    public class MultipleDeclarations
    {
        public void Method()
        {
            int first = 1, second = 2; // Noncompliant
            Console.WriteLine(second);
        }
    }

    public class AnonymousMethods
    {
        public void Method()
        {
            Action unused = delegate { };   // Noncompliant
            Action used = delegate { };
            used();

            Action<int> outer = delegate (int n)  // Compliant
            {
                int inner = 42; // Noncompliant
                Console.WriteLine(n.ToString());
            };
            outer(1);
        }
    }

    public class NameofExpression
    {
        public void Method()
        {
            var x = 1;
            Console.WriteLine(nameof(x)); // Compliant - nameof resolves the symbol
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
