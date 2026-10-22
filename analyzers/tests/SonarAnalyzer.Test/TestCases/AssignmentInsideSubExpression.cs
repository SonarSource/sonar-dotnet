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
            foo(val ?? 1);

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
            while ((evaluated = await GetTask().ConfigureAwait(false))) // Compliant
            {
                // do processing
            }
        }

        internal abstract Task<bool> GetTask();
    }

    // https://sonarsource.atlassian.net/browse/NET-4671
    public abstract class ParenthesizedLoopCondition
    {
        private bool field;
        public bool Flag { get; set; }

        internal abstract bool GetFlag();
        internal abstract Task<bool> GetFlagAsync();
        internal abstract int GetNumber();
        internal abstract bool Check(bool value);
        internal abstract void Consume(bool value);
        internal abstract void Consume(int value);

        public void While_Synchronous()
        {
            bool flag;
            while ((flag = GetFlag())) // Compliant
            {
                Consume(flag);
            }

            while (((flag = GetFlag()))) // Compliant
            {
                Consume(flag);
            }
        }

        public async Task While_Asynchronous()
        {
            bool flag;
            while ((flag = await GetFlagAsync())) // Compliant
            {
                Consume(flag);
            }

            while ((flag = await GetFlagAsync().ConfigureAwait(false))) // Compliant
            {
                Consume(flag);
            }
        }

        public void While_Field()
        {
            while ((field = GetFlag())) // Compliant
            {
                Consume(field);
            }
        }

        public void While_Property()
        {
            while ((Flag = GetFlag())) // Compliant
            {
                Consume(Flag);
            }
        }

        public void While_Indexer(bool[] flags, int index)
        {
            while ((flags[index] = GetFlag())) // Compliant
            {
                Consume(flags[index]);
            }
        }

        public void DoWhile_Synchronous()
        {
            bool flag = false;
            do
            {
                Consume(flag);
            }
            while ((flag = GetFlag())); // Compliant
        }

        public async Task DoWhile_Asynchronous()
        {
            bool flag = false;
            do
            {
                Consume(flag);
            }
            while ((flag = await GetFlagAsync())); // Compliant

            do
            {
                Consume(flag);
            }
            while ((flag = await GetFlagAsync().ConfigureAwait(false))); // Compliant
        }

        public void CompoundAssignments()
        {
            bool flag = false;
            while ((flag &= GetFlag())) { Consume(flag); } // Compliant
            while ((flag |= GetFlag())) { Consume(flag); } // Compliant
            while ((flag ^= GetFlag())) { Consume(flag); } // Compliant

            do { Consume(flag); } while ((flag &= GetFlag())); // Compliant
            do { Consume(flag); } while ((flag |= GetFlag())); // Compliant
            do { Consume(flag); } while ((flag ^= GetFlag())); // Compliant
        }

        public void LiteralAssignments()
        {
            bool flag;
            while ((flag = false)) { Consume(flag); } // Noncompliant {{Extract the assignment of 'flag' from this expression.}}
            while ((flag = true)) { Consume(flag); } // Noncompliant
            while (((flag = (false)))) { Consume(flag); } // Noncompliant
            while (((flag = (true)))) { Consume(flag); } // Noncompliant

            do { Consume(flag); } while ((flag = false)); // Noncompliant
            do { Consume(flag); } while ((flag = true)); // Noncompliant
            do { Consume(flag); } while (((flag = (false)))); // Noncompliant
            do { Consume(flag); } while (((flag = (true)))); // Noncompliant

            while ((flag = default(bool))) { Consume(flag); } // Noncompliant
            while (((flag = (default(bool))))) { Consume(flag); } // Noncompliant
            do { Consume(flag); } while ((flag = default(bool))); // Noncompliant
            do { Consume(flag); } while (((flag = (default(bool))))); // Noncompliant
        }

        public void EqualityComparisons(bool flag)
        {
            while ((flag == true)) // Compliant
            {
                flag = GetFlag();
            }

            while ((flag == false)) // Compliant
            {
                flag = GetFlag();
            }

            do
            {
                flag = GetFlag();
            }
            while ((flag == true)); // Compliant

            do
            {
                flag = GetFlag();
            }
            while ((flag == false)); // Compliant
        }

        public void WithoutExplicitParentheses()
        {
            bool flag;
            while (flag = GetFlag()) // Noncompliant
            {
                Consume(flag);
            }

            do
            {
                Consume(flag);
            }
            while (flag = GetFlag()); // Noncompliant
        }

        public void OtherStatementConditions()
        {
            bool flag;
            if ((flag = GetFlag())) // Noncompliant
            {
                Consume(flag);
            }

            for (; (flag = GetFlag());) // Noncompliant
            {
                Consume(flag);
            }
        }

        public void NestedExpressions(bool ready)
        {
            bool flag;
            while (Check((flag = GetFlag()))) // Noncompliant
            {
                Consume(flag);
            }

            while ((flag = GetFlag()) && ready) // Noncompliant
            {
                Consume(flag);
            }

            while ((flag = GetFlag()) || ready) // Noncompliant
            {
                Consume(flag);
            }

            while (!(flag = GetFlag())) // Noncompliant
            {
                Consume(flag);
            }
        }

        public void Comparisons()
        {
            bool flag;
            while ((flag = GetFlag()) == true) // Compliant
            {
                Consume(flag);
            }

            do
            {
                Consume(flag);
            }
            while ((flag = GetFlag()) == true); // Compliant

            int number;
            while ((number = GetNumber()) > 0) // Compliant
            {
                Consume(number);
            }
        }
    }
}
