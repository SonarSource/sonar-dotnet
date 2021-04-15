using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class EmptyNullableValueAccess
    {
        protected void LogFailure(Exception e)
        {
            try
            {
                var message = e == null ? null : e;
            }
            finally { }
        }

        private IEnumerable<TestClass> numbers = new[]
        {
            new TestClass { Number = 42 },
            new TestClass(),
            new TestClass { Number = 1 },
            new TestClass { Number = null }
        };

        public class TestClass
        {
            public int? Number { get; set; }
        }

        void Nameof(object o)
        {
            if (o == null)
            {
            }

            if (o == nameof(Object))
            {
                o.ToString(); // Compliant
            }
        }

        private int i0;
        public void SetI0()
        {
            i0 = 42;
        }

        public void TestNull()
        {
            int? i1 = null;
            if (i1.HasValue)
            {
                Console.WriteLine(i1.Value);
            }

            Console.WriteLine(i1.Value); // Noncompliant {{'i1' is null on at least one execution path.}}
//                            ^^^^^^^^
        }

        public IEnumerable<TestClass> TestEnumerableExpressionWithCompilableCode() => numbers.OrderBy(i => i.Number.HasValue).ThenBy(i => i.Number);
        public IEnumerable<int> TestEnumerableExpressionWithNonCompilableCode() => numbers.OrderBy(i => i.Number.HasValue).ThenBy(i => i.Number).Select(x => x.Number ?? 0);

        public void TestNonNull()
        {
            int? i1 = 42;
            if (i1.HasValue)
            {
                Console.WriteLine(i1.Value);
            }

            Console.WriteLine(i1.Value);
        }

        public void TestNullConstructor()
        {
            int? i2 = new Nullable<int>();
            if (i2.HasValue)
            {
                Console.WriteLine(i2.Value);
            }

            Console.WriteLine(i2.Value); // Noncompliant
        }

        public void TestNonNullConstructor()
        {
            int? i1 = new Nullable<int>(42);
            if (i1.HasValue)
            {
                Console.WriteLine(i1.Value);
            }

            Console.WriteLine(i1.Value);
        }

        public void TestComplexCondition(int? i3, double? d3, float? f3)
        {
            if (i3.HasValue && i3.Value == 42)
            {
                Console.WriteLine();
            }

            if (!i3.HasValue && i3.Value == 42) // TODO: Should be NC
            {
                Console.WriteLine();
            }

            if (!d3.HasValue)
            {
                Console.WriteLine(d3.Value); // TODO: Should be NC
            }

            if (f3 == null)
            {
                Console.WriteLine(f3.Value); // TODO: Should be NC
            }
        }

        public int CSharp8_SwitchExpressions(bool zero)
        {
            int? i = zero switch { true => 0, _ => null };
            return i.Value; // Noncompliant
        }

        public int CSharp8_SwitchExpressions2(bool zero)
        {
            int? i = zero switch { true => 0, _ => 1 };
            return i.Value;
        }

        public int CSharp8_SwitchExpressions3(int? value)
        {
            return value.HasValue switch { true => value.Value, false => 0};
        }

        public int CSharp8_SwitchExpressions4(int? value)
        {
            return value.HasValue switch { true => 0, false => value.Value };   // FN - switch expressions are not constrained
        }

        public int CSharp8_SwitchExpressions5(int? value, bool flag)
        {
            return flag switch { true => value.Value, false => 0 }; // FN - switch expressions are not constrained
        }

        public int CSharp8_StaticLocalFunctions(int? param)
        {
            static int ExtractValue(int? intOrNull)
            {
                return intOrNull.Value; // FN - content of static local function is not inspected by SE
            }

            return ExtractValue(param);
        }

        public int CSharp8_NullCoalescingAssignment(int? param)
        {
            param ??= 42;
            return param.Value; // OK, value is always set
        }

    }

    class TestLoopWithBreak
    {
        public static void LoopWithBreak(System.Collections.Generic.IEnumerable<string> list, bool condition)
        {
            int? i1 = null;
            foreach (string x in list)
            {
                try
                {
                    if (condition)
                    {
                        Console.WriteLine(i1.Value); // Noncompliant
                    }
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }

    public interface IWithDefaultImplementation
    {
        int DoSomething()
        {
            int? i = null;
            return i.Value; //Noncompliant
        }
    }

}
