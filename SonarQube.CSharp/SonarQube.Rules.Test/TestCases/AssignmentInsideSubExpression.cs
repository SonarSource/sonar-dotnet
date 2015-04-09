using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class AssignmentInsideSubExpression
    {
        void foo(int a)
        {
        }

        void foo(bool a)
        {
        }

        int foo(Func<int, int> f)
        {
            throw new Exception();
        }

        private class MyClass
        {
            public int MyField;
        }

        void Foo()
        {
            int i = 0;

            foo(i = 42); // Noncompliant
            foo(i += 42); // Noncompliant
            foo(i -= 42); // Noncompliant
            foo(i *= 42); // Noncompliant
            foo(i /= 42); // Noncompliant
            foo(i %= 42); // Noncompliant
            foo(i &= 1); // Noncompliant
            foo(i ^= 1); // Noncompliant
            foo(i |= 1); // Noncompliant
            foo(i <<= 1); // Noncompliant
            foo(i >>= 1); // Noncompliant

            i = 42;
            foo(i == 42);

            foo(
                (int x) =>
            {
                int a;
                a = 42;
                return a;
            });

            var b = true;

            if (b = false) { } // Not yet covered
            if (i == 0) i = 2;

            string result = "";
            if (!string.IsNullOrEmpty(result)) result = result + " ";
            var v1 = new Action(delegate { });
            var v2 = new Action(delegate { var foo = 42; });
            var v3 = new Func<object, object>((x) => x = 42);
            var v4 = new Action<object>((x) => { x = 42; });
            var v5 = new { MyField = 42 };
            var v6 = new MyClass { MyField = 42 };
            var v7 = new MyClass() { MyField = 42 };
            var v8 = foo(x => { x = 42; return 0; } );
        }
    }
}
