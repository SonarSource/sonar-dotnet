using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class BooleanLiteralUnnecessary
    {
        public BooleanLiteralUnnecessary(bool a, bool b, bool? c, Item item)
        {
            var z = true || ((true));   // Noncompliant {{Remove the unnecessary Boolean literal(s).}}
//                       ^^^^^^^^^^^
            z = (true) || true;     // Noncompliant
//                     ^^^^^^^
            z = false || false;     // Noncompliant (also S2589 and S1764)
            z = true || false;      // Noncompliant
            z = false || true;      // Noncompliant
            z = false && false;     // Noncompliant
            z = false && true;      // Noncompliant
            z = true && true;       // Noncompliant
            z = true && false;      // Noncompliant
            z = true == true;       // Noncompliant
            z = false == true;      // Noncompliant
            z = false == false;     // Noncompliant
            z = true == false;      // Noncompliant
            z = (true == false);    // Noncompliant
            z = true != true;       // Noncompliant (also S1764)
            z = false != true;      // Noncompliant
            z = false != false;     // Noncompliant
            z = true != false;      // Noncompliant
            z = true is true;       // Noncompliant
            z = false is true;      // Noncompliant
            z = false is false;     // Noncompliant
            z = true is false;      // Noncompliant

            var x = !true;                  // Noncompliant
//                   ^^^^
            x = true || false;              // Noncompliant
            x = !false;                     // Noncompliant
            x = (a == false)                // Noncompliant
                && true;                    // Noncompliant
            x = (a is false)                // Noncompliant
                && true;                    // Noncompliant

            x = a is (true);                // Noncompliant
            x = a == true;                  // Noncompliant
            x = a is true;                  // Noncompliant
            x = a != false;                 // Noncompliant
            x = a != true;                  // Noncompliant
            x = false == a;                 // Noncompliant
            x = true == a;                  // Noncompliant
            x = false is a;                 // Error [CS9135]
            x = true is a;                  // Error [CS9135]
            x = false != a;                 // Noncompliant
            x = true != a;                  // Noncompliant
            x = false && Foo();             // Noncompliant
//                    ^^^^^^^^
            x = Foo() && false;             // Noncompliant
//              ^^^^^^^^
            x = true && Foo();             // Noncompliant
//              ^^^^^^^
            x = Foo() && true;             // Noncompliant
//                    ^^^^^^^
            x = Foo() || false;              // Noncompliant
//                    ^^^^^^^^
            x = false || Foo();              // Noncompliant
//              ^^^^^^^^
            x = Foo() || true;              // Noncompliant
//              ^^^^^^^^
            x = true || Foo();              // Noncompliant
//                   ^^^^^^^^
            x = a == true == b;             // Noncompliant

            x = a == Foo(((true)));             // Compliant
            x = !a;                         // Compliant
            x = Foo() && Bar();             // Compliant

            var condition = false;
            var exp = true;
            var exp2 = true;

            var booleanVariable = condition ? ((true)) : exp; // Noncompliant
//                                            ^^^^^^^^
            booleanVariable = condition ? false : exp; // Noncompliant
            booleanVariable = condition ? exp : true; // Noncompliant
            booleanVariable = condition ? exp : false; // Noncompliant
            booleanVariable = condition ? true : false; // Noncompliant
            booleanVariable = !condition ? true : false; // Noncompliant
            booleanVariable = condition ? true : true; // Compliant, this triggers another issue S2758
            booleanVariable = condition ? throw new Exception() : true; // Compliant, we don't raise for throw expressions
            booleanVariable = condition ? throw new Exception() : false; // Compliant, we don't raise for throw expressions

            booleanVariable = condition ? exp : exp2;

            b = x || booleanVariable ? false : true; // Noncompliant

            SomeFunc(true || true); // Noncompliant

            if (c == true) //Compliant
            { }
            if (b is true) // Noncompliant
//                ^^^^^^^
            { }
            if (b is false) // Noncompliant
//                ^^^^^^^^
            { }
            if (c is true) // Compliant
            { }

            var d = true ? c : false;

            var newItem = new Item
            {
                Required = item == null ? false : item.Required // Noncompliant
//                                        ^^^^^
            };
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/2618
        public void Repro_2618(Item item)
        {
            var booleanVariable = item is Item myItem ? myItem.Required : false; // Noncompliant
            booleanVariable = item is Item myItem2 ? myItem2.Required : true; // Noncompliant
        }

        public static void SomeFunc(bool x) { }

        private bool Foo()
        {
            return false;
        }

        private bool Foo(bool a)
        {
            return a;
        }

        private bool Bar()
        {
            return false;
        }

        private void M()
        {
            for (int i = 0; true; i++) // Noncompliant
            {
            }
            for (int i = 0; false; i++)
            {
            }
            for (int i = 0; ; i++)
            {
            }

            var b = true;
            for (int i = 0; b; i++)
            {
            }
        }

        private void IsPattern(bool a, bool c)
        {
            const bool b = true;
            a = a is b;
            a = (a is b) ? a : b;
            a = (a is b && c) ? a : b;
            a = a is b && c;

            if (a is bool d
                && a is var e)
            { }
        }

        // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/4465
        private class Repro4465
        {
            public int LiteralInTernaryCondition(bool condition, int result)
            {
                return condition == false
                    ? result
                    : throw new Exception();
            }

            public bool LiteralInTernaryBranch(bool condition)
            {
                return condition
                    ? throw new Exception()
                    : true;
            }

            public void ThrowExpressionIsIgnored(bool condition, int number)
            {
                var x = !condition ? throw new Exception() : false;
                x = number > 0 ? throw new Exception() : false;
                x = condition && condition ? throw new Exception() : false;
                x = condition || condition ? throw new Exception() : false;
                x = condition != true ? throw new Exception() : false;
                x = condition is true ? throw new Exception() : false;
            }
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/7462
        public void Repro_7462(object obj)
        {
            var a = obj == null ? false : obj.ToString() == "x" || obj.ToString() == "y"; // Noncompliant
        }
    }

    public class Item
    {
        public bool Required { get; set; }
    }

    public class SocketContainer
    {
        private IEnumerable<Socket> sockets;
        public bool IsValid =>
            sockets.All(x => x.IsStateValid == true); // Compliant, this failed when we compile with Roslyn 1.x
    }

    public class Socket
    {
        public bool? IsStateValid { get; set; }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7999
    class Repro7999CodeFixError
    {
        void Method(bool cond)
        {
            if (cond == false) { } // Noncompliant, TP but code fix is wrong - it should be fixed to "if (!cond)"
        }
    }
}
