using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            foo(i = 42); // Noncompliant {{Extract the assignment of 'i' from this expression.}}
//                ^
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
//                ^^^

            int? val = null;
            foo(val ??= 1); // Compliant, see e.g. https://stackoverflow.com/a/64666607
            foo(val ?? 1);
            val ??= 1;

            i = 42;
            foo(i == 42);

            foo(
                (int xx) =>
                {
                    int a;
                    a = 42;
                    return a;
                });

            var b = true;

            if (b = false) { } // Noncompliant
//                ^
            if ((b = false)) { } // Noncompliant  {{Extract the assignment of 'b' from this expression.}}
            for (int j = 0; b &= false; j++) { } // Noncompliant
            for (int j = 0; b == false; j++) { }

            bool? value = null;
            if (value ??= true) { } // Compliant, see. e.g. https://stackoverflow.com/a/64666607
            if (value ?? true) { }

            // Fix S1121: NullReferenceException when while loop with assignment expression is within a for loop with no condition (#725)
            for (;;)
            {
                while ((b = GetBool()) == true) { }
            }

            while (b &= false) { } // Noncompliant

            do { } while (b &= false); // Noncompliant

            while ((i = 1) == 1) { } // Compliant

            do { } while ((i = 1) == 1); // Compliant

            if (i == 0) i = 2;
            if ((i = 1) <= 1) // Compliant
            {
            }
            b = (i = 1) <= 1; // Noncompliant

            var y = (b &= false) ? i : i * 2; // Noncompliant

            string result = "";
            if (!string.IsNullOrEmpty(result)) result = result + " ";
            var v1 = new Action(delegate { });
            var v2 = new Action(delegate { var foo = 42; });
            var v3 = new Func<object, object>((xx) => xx = 42);
            var v4 = new Action<object>((xx) => { xx = 42; });
            var v5 = new { MyField = 42 };
            var v6 = new MyClass { MyField = 42 };
            var v7 = new MyClass() { MyField = 42 };
            var v8 = foo(xx => { xx = 42; return 0; });
            var v9 = new MyClass { MyField = i = 42 }; // Noncompliant
            var v10 = new MyClass() { MyField = i = 42 }; // Noncompliant

            var index = 0;
            new int[] { 0, 1, 2 }[index = 2] = 10; // Noncompliant
            new int[] { 0, 1, 2 }[(index = 2)] = 10; // Noncompliant

            var o = new object();
            var oo = new object();

            if (false && (oo = o) != null) // Compliant
            { }

            oo = (o) ?? (o = new object()); // Compliant
            oo = (o) ?? (object)(o = new object()); // Compliant

            oo = oo ?? (o = new object()); // Noncompliant
            int xa = 0, xb = 0;
            if ((xa = xb = 0) != 0) { }  // Compliant
            int x = (xa = xb) + 5; // Noncompliant
        }
        public void TestMethod1()
        {
            var j = 5;
            var k = 5;
            var i = j =
                k = 10;
            i = j =
                k = 10;
        }

        public bool GetBool() => true;
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/4446
    abstract class WaitingInLoop
    {
        public async void Process()
        {
            bool evaluated;
            while ((evaluated = await GetTask().ConfigureAwait(false))) // Noncompliant FP
            {
                // do processing
            }
        }

        internal abstract Task<bool> GetTask();
    }
}
