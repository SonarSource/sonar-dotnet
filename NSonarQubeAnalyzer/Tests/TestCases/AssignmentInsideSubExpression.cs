using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class AssignmentInsideSubExpression
    {
        void Foo(int a)
        {
        }

        void Foo(bool a)
        {
        }

        void Foo(Func<int, int> f)
        {
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

            if (i = 0) { } // Not yet covered
            if (i == 0) i = 2;

            string result = "";
            if (!string.IsNullOrEmpty(result)) result = result + " ";
            var v1 = delegate { };
            var v2 = delegate { foo = 42; };
            var v3 = (x) => x = 42;
            var v4 = (x) => { x = 42; };
            var v5 = new { MyField = 42 };
            var v6 = new MyClass { MyField = 42 };
            var v7 = new MyClass() { MyField = 42 };
            var v8 = Foo(x => { x = 42; return 0; } );
        }
    }
}
