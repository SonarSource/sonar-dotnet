using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class ListTests 
    {
        private IEnumerable<int> items = new List<int> { 1, 2, 3 };

        private static bool Predicate(int i) => true;
        private static bool Action(int i) { }
        private List<int> Factory() => new List<int>();

        public void DefaultConstructor_Applies_Empty()
        {
            var list = new List<int>();
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithCapacity_Applies_Empty()
        {
            var list = new List<int>(5);
            list.Clear(); // Noncompliant
        }

        public void ConstructorWithEnumerable_Applies_NonEmpty()
        {
            var list = new List<int>(items);
            list.Clear(); // Compliant
        }

        public void ConstructorWithInitializer_Applies_NonEmpty()
        {
            var list = new List<int> { 1, 2, 3 };
            list.Clear(); // Compliant
        }

        public void ConstructorWithEmptyInitializer_Applies_Empty()
        {
            var list = new List<int> { };
            list.Clear(); // Noncompliant
        }

        public void Other_Initialization_Applies_No_Constraints()
        {
            var list = Factory();
            list.Clear(); // Compliant, we don't know anything about the list
        }

        public void Methods_Raise_Issue()
        {
            var list = new List<int>();
            list.Clear(); // Noncompliant
            list.Contains(1); // Noncompliant
            list.Exists(Predicate); // Noncompliant
            list.Find(Predicate); // Noncompliant
            list.FindIndex(Predicate); // Noncompliant
            list.ForEach(Action); // Noncompliant
            list.IndexOf(1); // Noncompliant
            list.Remove(1); // Noncompliant
            list.RemoveAll(Predicate); // Noncompliant
            list.Reverse(); // Noncompliant
            list.Sort(); // Noncompliant
            var x = list[5]; // Noncompliant
            list[5] = 5; // Noncompliant
        }

        public void Methods_Ignored()
        {
            var list = new List<int>();
            list.AsReadOnly();
            list.ToArray();
            list.GetHashCode();
            list.Equals(items);
            list.GetType();
            list.ToString();
        }

        public void Methods_Set_NotEmpty()
        {
            var list = new List<int>();
            list.Add(1);
            list.Clear(); // Compliant, will normally raise

            list = new List<int>();
            list.AddRange(items);
            list.Clear(); // Compliant

            list = new List<int>();
            list.Insert(0, 1);
            list.Clear(); // Compliant

            list = new List<int>();
            list.InsertRange(0, items);
            list.Clear(); // Compliant
        }
    }
}
