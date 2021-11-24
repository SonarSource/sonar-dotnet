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
        const double ConstField_Double_Zero = 0;
        const int ConstField_Zero = 0;
        const int ConstField_NonZero = 1;

        public void TestCountMethod()
        {
            const int localConst_Zero = 0;
            const int localConst_NonZero = 1;
            int localVariable = 0;
            bool result;

            var someEnumerable = new List<string>();

            result = someEnumerable.Count() >= 0; // Noncompliant {{The 'Count' of 'IEnumerable<T>' always evaluates as 'True' regardless the size.}}
            //       ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            result = someEnumerable.Count(foo => true) >= 0; // Noncompliant
            result = someEnumerable?.Count() >= 0; // Noncompliant

            result = someEnumerable.Count() >= 1;
            result = someEnumerable.Count() >= localVariable;
            result = someEnumerable.Count() >= -1; // Noncompliant {{The 'Count' of 'IEnumerable<T>' always evaluates as 'True' regardless the size.}}
            result = someEnumerable.Count() <= 0;
            result = someEnumerable.Count() < 0; // Noncompliant {{The 'Count' of 'IEnumerable<T>' always evaluates as 'False' regardless the size.}}
            result = 0 >= someEnumerable.Count();

            result = someEnumerable.Count() >= localConst_Zero; // Noncompliant
            result = someEnumerable.Count() >= ConstField_NonZero;
            result = someEnumerable.Count() >= ConstField_Zero; // Noncompliant
            result = someEnumerable.Count() >= localConst_NonZero;
            result = someEnumerable.Count() >= ConstField_Double_Zero; // Compliant, double is ignored, arguably FN

            result = (someEnumerable.Count()) >= (0); // Noncompliant
            result = ((((someEnumerable).Count())) >= ((0))); // Noncompliant
            //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            result = 0 <= someEnumerable.Count(); // Noncompliant
            //       ^^^^^^^^^^^^^^^^^^^^^^^^^^^

            result = result.ToString() == "Not integer";
            result = (localVariable = 42) != -1;

            if ((localVariable = 42) != -1) { }

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
            bool result = someCollection.Count >= 0; // Noncompliant {{The 'Count' of 'ICollection' always evaluates as 'True' regardless the size.}}
            //            ^^^^^^^^^^^^^^^^^^^^^^^^^

            var nonCollection = new FooProperty();
            result = nonCollection.Count >= 0;
        }

        public void TestLengthProperty()
        {
            var someArray = new string[0];
            bool result;

            result = someArray.Length >= 0; // Noncompliant {{The 'Length' of 'Array' always evaluates as 'True' regardless the size.}}
            //       ^^^^^^^^^^^^^^^^^^^^^

            result = someArray.LongLength >= 0; // Noncompliant {{The 'LongLength' of 'Array' always evaluates as 'True' regardless the size.}}
            //       ^^^^^^^^^^^^^^^^^^^^^^^^^

            var nonArray = new FooProperty();
            result = nonArray.Length >= 0;
            result = nonArray.LongLength >= 0;
        }

        private void TestInterfacesAndReadonlyCollections(IList<int> list, ICollection<int> collection, IReadOnlyCollection<int> readonlyCollection, IReadOnlyList<int> readonlyList)
        {
            SortedSet<double> sortedSet = new SortedSet<double>();

            bool result;

            result = list.Count >= 0; // Noncompliant

            result = collection.Count >= 0; // Noncompliant

            result = readonlyCollection.Count >= 0; // Noncompliant

            result = readonlyList.Count >= 0; // Noncompliant

            result = sortedSet.Count >= 0; // Noncompliant
        }
    }

    class OnString
    {
        static bool LengthWithoutMeaning(string str)
        {
            return str.Length < -3; // Noncompliant {{The 'Length' of 'String' always evaluates as 'False' regardless the size.}}
        }
    }
}
