using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    class FooProperty
    {
        public int Length => 0;

        public int LongLength => 0;

        public int Count => 0;
    }

    class FooMethod
    {
        public int Count() => 0;
    }

    class DummyHolder
    {
        public List<string> Enumerable;
        public List<string> GetEnumerable() { return null; }

        public string[] Array;
        public string[] GetArray() { return null; }

        public DummyHolder Holder;
        public DummyHolder GetHolder() { return null; }
    }

    class Program
    {
        const int ConstField_Zero = 0;
        const int ConstField_NonZero = 1;

        public void TestCountMethod()
        {
            const int localConst_Zero = 0;
            const int localConst_NonZero = 1;
            int localVariable = 0;
            bool result;

            var someEnumerable = new List<string>();

            result = someEnumerable.Count() >= 0; // Noncompliant {{The count of 'IEnumerable<T>' is always '>=0', so fix this test to get the real expected behavior.}}
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            result = someEnumerable.Count(foo => true) >= 0; // Noncompliant
            result = someEnumerable?.Count() >= 0; // Noncompliant

            result = someEnumerable.Count() >= 1;
            result = someEnumerable.Count() >= localVariable;
            result = someEnumerable.Count() >= -1;
            result = someEnumerable.Count() <= 0;
            result = someEnumerable.Count() < 0;
            result = 0 >= someEnumerable.Count();

            result = someEnumerable.Count() >= localConst_Zero; // Noncompliant
            result = someEnumerable.Count() >= ConstField_NonZero;
            result = someEnumerable.Count() >= ConstField_Zero; // Noncompliant
            result = someEnumerable.Count() >= localConst_NonZero;

            result = (someEnumerable.Count()) >= (0); // Noncompliant
            result = ((((someEnumerable).Count())) >= ((0))); // Noncompliant
//                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            result = 0 <= someEnumerable.Count(); // Noncompliant
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var nonEnumerable = new FooMethod();
            result = nonEnumerable.Count() >= 0;
        }

        public void TestComplexAccess()
        {
            var someArray = new string[0];
            var someEnumerable = new List<string>();
            var holder = new DummyHolder();

            bool result;
            result = holder.GetHolder().GetHolder().GetArray().Count() >= 0; // Noncompliant
            result = holder.GetHolder()?.GetHolder()?.GetArray()?.Length >= 0; // Noncompliant
            result = holder.GetHolder()?.GetHolder().Holder.Array.Length >= 0; // Noncompliant
            result = holder.GetHolder()?.GetHolder().Holder.Enumerable.Count(foo => true) >= 0; // Noncompliant
            result = (holder.GetHolder()?.GetHolder())?.GetArray()?.Length >= 0; // Noncompliant
        }

        public void TestCountProperty()
        {
            var someCollection = new List<string>();
            bool result = someCollection.Count >= 0; // Noncompliant {{The count of 'ICollection' is always '>=0', so fix this test to get the real expected behavior.}}
//                        ^^^^^^^^^^^^^^^^^^^^^^^^^

            var nonCollection = new FooProperty();
            result = nonCollection.Count >= 0;
        }

        public void TestLengthProperty()
        {
            var someArray = new string[0];
            bool result;

            result = someArray.Length >= 0; // Noncompliant {{The length of 'Array' is always '>=0', so fix this test to get the real expected behavior.}}
//                   ^^^^^^^^^^^^^^^^^^^^^

            result = someArray.LongLength >= 0; // Noncompliant {{The longlength of 'Array' is always '>=0', so fix this test to get the real expected behavior.}}
//                   ^^^^^^^^^^^^^^^^^^^^^^^^^

            var nonArray = new FooProperty();
            result = nonArray.Length >= 0;
            result = nonArray.LongLength >= 0;
        }
    }
}
