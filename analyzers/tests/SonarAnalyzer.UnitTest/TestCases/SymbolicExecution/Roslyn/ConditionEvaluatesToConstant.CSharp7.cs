using System;

namespace Tests.Diagnostics
{
    public class ExpressionBodyTest
    {
        void ConstantPattern(object o)
        {
            if (o == null)
            {
                if (o is null)      // Noncompliant, always true
                {
                    o.ToString();
                }
                else
                {
                    o.ToString();   // Secondary
                }
            }
        }

        void ConstantPattern_NotIsNull(object o)
        {
            if (o == null)
            {
                if (!(o is null))   // Noncompliant, always false
                {
                    o.ToString();   // Secondary
                }
                else
                {
                    o.ToString();
                }
            }
        }

        void VariableDesignationPattern_Variable(object o)
        {
            if (o is string s)
            {
                if (s == null)      // Noncompliant, always false
                {
                    s.ToString();   // Secondary
                }
            }
        }

        void VariableDesignationPattern_Source(object o)
        {
            if (o is string s)
            {
                if (o == null)      // Noncompliant, always false
                {
                    o.ToString();   // Secondary
                }
            }
        }

        void Patterns_In_Loops(object o, object[] items)
        {
            while (o is string s)
            {
                if (s == null)      // Noncompliant, always false
                {
                    s.ToString();   // Secondary
                }
            }

            do
            {
                // The condition is evaluated after the first execution, so we cannot test s
            }
            while (o is string s);

            for (int i = 0; i < items.Length && items[i] is string s; i++)
            {
                if (s != null)      // Noncompliant, always true
                {
                    s.ToString();
                }
            }
        }

        void Switch_Pattern_Source(object o)
        {
            switch (o)
            {
                case string s:
                    if (o == null)      // Noncompliant: always false
                    {
                        o.ToString();   // Secondary
                    }
                    break;

                default:
                    break;
            }
        }

        void Switch_Pattern(object o)
        {
            switch (o)
            {
                case string s:
                    if (s == null)      // Noncompliant, always false
                    {
                        s.ToString();   // Secondary
                    }
                    break;

                case int _:             // The discard is redundant, but still allowed
                case null:
                    if (o == null)      // Compliant, False Negative
                    {
                    }

                    break;
                default:
                    break;
            }
        }

        void Switch_Pattern_Constant_With_When(int i)
        {
            switch (i)
            {
                case 0 when i == 0: // the when is redundant, but needed to convert the case to pattern
                    break;
                default:
                    break;
            }
        }

        public class A
        {
            public object booleanVal { get; set; }
        }

        void Compliant1(A a)
        {
            if (a?.booleanVal is null)
            {

            }
        }

        void NonCompliant1()
        {
            A a = null;
            if (a?.booleanVal is null)
            //  ^                       Noncompliant
            //    ^^^^^^^^^^^           Secondary@-1
            //  ^^^^^^^^^^^^^^^^^^^^^   Noncompliant@-2
            {

            }
        }

    }


    // https://github.com/SonarSource/sonar-dotnet/issues/2592
    public class LoopsAreNotVisited
    {
        public void DoWhileWithPattern(bool cond)
        {
            bool done;
            do
            {
                done = cond;
            }
            while (done is false); // Compliant
        }

        public void DoWhile(bool cond)
        {
            bool done;
            do
            {
                done = cond;
            }
            while (done == false); // Compliant
        }

        public static void M(string path, int timeoutmilliseconds = 500)
        {
            bool deletesuccess = false;
            do
            {
                try
                {
                    deletesuccess = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(timeoutmilliseconds);
                }
            } while (!deletesuccess);   // Noncompliant: This repro is written badly. Look below for a fixed version.
                                        // This is a TP because the assignment cannot throw => deletesuccess is always true.
        }

        public static void M_Fixed(string path, int timeoutmilliseconds = 500)
        {
            bool deletesuccess = false;
            do
            {
                try
                {
                    Console.WriteLine("May throw");
                    deletesuccess = true;
                }
                catch
                {
                    System.Threading.Thread.Sleep(timeoutmilliseconds);
                }
            } while (!deletesuccess);   // Compliant: Console.WriteLine can throw
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2590
    static class Repro2590
    {
        static void Main()
        {
            Foo foo = null;

            try
            {
                foo = new Foo(Guid.Empty);
                foo.Write();
            }
            catch
            {
                // Do nothing
            }

            if (foo == null) // Compliant
            {
                Console.WriteLine("Foo is null");
            }

            if (foo != null) // Compliant
            {
                Console.WriteLine("Foo is not null");
            }
        }

        class Foo
        {
            private Guid Id { get; }

            public Foo(Guid id)
            {
                Id = id;
            }

            public void Write()
            {
                Console.WriteLine(Id);
            }
        }
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/3110
    public class Repro_3110
    {
        public int DoSomething(object value)
        {
            if (!(value is bool valueBool))
                return -1;

            if (valueBool)  // Compliant - valueBool can be true or false
            {
                return 42;
            }
            else
            {
                return 0;
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3123
    public class Repro_3123
    {
        public bool FooA(object x) => x is bool value && value;
        public bool FooB(object x) => x is bool value && value == true;
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3239
    public class Repro_3239
    {
        public void GoGoGo()
        {
            var tmp = 0;
            var flag = true;
            while (flag)            // Compliant
            {
                (flag, tmp) = (false, 5);
            }
        }

        public void Tuple()
        {
            var tmp = 0;
            var flag = true;
            while (flag)                    // Noncompliant
            {
                tmp = 0;
            }

            while (flag)                    // Secondary
            {
                (flag, tmp) = (false, 5);
            }
        }

        public void TupleFalse()
        {
            var tmp = 0;
            var flag = false;
            while (flag)                    // Noncompliant
            {
                (flag, tmp) = (false, 5);   // Secondary
            }
        }

        public void MutedNull()
        {
            var tmp = 0;
            var flag = "x";
            while (flag != null)            // Compliant
            {
                (flag, tmp) = (null, 5);
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3288
    public class Repro_3288
    {
        public static void DoSomething1(object value) =>
            DoSomething(
                value is null ? 1 : 2,
                value is bool b ? 3 : 4,
                value is null ? 5 : 6); // Compliant

        public static void IsBoolWithoutB(object value) =>
            DoSomething(
                value is null ? 1 : 2,
                value is bool ? 3 : 4,  // Variable 'b' is removed
                value is null ? 5 : 6); // OK, this doesn't reproduce the issue

        public static void IsBoolB(object value) =>
            DoSomething(
                0,
                value is bool b ? 3 : 4,
                value is null ? 5 : 6); // OK, this doesn't reproduce the issue

        public static void IsNull(object value) =>
            DoSomething(
                value is null ? 1 : 2,
                0,
                value is null ? 5 : 6); // OK, this doesn't reproduce the issue

        private static void DoSomething(int a, int b, int c) { }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/2528
    public class Repro_2528
    {
        public int Count { get; }
        public int Foo()
        {
            Repro_2528 x = null;
            if (x == null) { }          // Noncompliant
            if ((x?.Count ?? 0) == 0)
            //   ^                          Noncompliant
            //     ^^^^^^                   Secondary@-1
            //   ^^^^^^^^                   Noncompliant@-2
            //  ^^^^^^^^^^^^^^^^^^^^        Noncompliant@-3
            {
                return -1;
            }

            return 1;                   // Secondary
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3910
    public class Repro_3910
    {
        public void ElementAccess1(Exception ex)
        {
            if (ex?.Data["Key"] is string value)
            {
            }
            else if (ex != null)            // Compliant
            {
            }
        }

        public void MemberBinding1(Exception ex)
        {
            if (ex?.Message is string value)
            {
            }
            else if (ex != null)            // Compliant
            {
            }
        }

        public void MemberBinding2(Exception ex)
        {
            if (ex?.Message is string value)
            {
            }
            else if (ex == null)            // Compliant
            {
            }
            else if (ex.Message != null)    // FN
            {
            }
        }
    }

    // See: https://github.com/SonarSource/sonar-dotnet/issues/4559
    public class ImplicitOperator
    {
        public class MyClass
        {
            public static implicit operator string(MyClass value) => null;

            public static implicit operator bool?(MyClass value) => null;
        }

        public void Foo()
        {
            var value = new MyClass();
            string str = value;

            if (str == null)    // Compliant
            {
                Console.WriteLine("null");
            }

            bool? b = value;
            if (b == null)      // Compliant
            {
                Console.WriteLine("null");
            }
        }
    }
}
