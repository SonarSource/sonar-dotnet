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
    }
}
