using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    class Program : IEqualityComparer<int>
    {
        public bool Equals(int left, int right) => left == right;

        public int GetHashCode(int obj) => 42;

        void UnexpectedBehavior()
        {
            var list = new List<int>();
            var set = new HashSet<int>();

            list.AddRange(list);           // Compliant, see: https://sonarsource.atlassian.net/browse/NET-1729
            list.Concat(list);             // Compliant, see: https://sonarsource.atlassian.net/browse/NET-1729
            Enumerable.Concat(list, list); // Compliant, see: https://sonarsource.atlassian.net/browse/NET-1729
        }

        void AlwaysSameCollection()
        {
            var list = new List<int>();
            var set = new HashSet<int>();

            list.Union(list); // Noncompliant {{Change one instance of 'list' to a different value; This operation always produces the same collection.}}
            // Secondary@-1
            list.Union(list, this); // Noncompliant
            // Secondary@-1
            Enumerable.Union(list, list); // Noncompliant
            // Secondary@-1
            list.Intersect(list); // Noncompliant
            // Secondary@-1
            Enumerable.Intersect(list, list); // Noncompliant
            // Secondary@-1
            set.UnionWith(set); // Noncompliant
            // Secondary@-1
            set.IntersectWith(set); // Noncompliant
            // Secondary@-1
        }

        void AlwaysEmptyCollection()
        {
            var list = new List<int>();
            var set = new HashSet<int>();

            list.Except(list); // Noncompliant {{Change one instance of 'list' to a different value; This operation always produces an empty collection.}}
            // Secondary@-1
            Enumerable.Except(list, list); // Noncompliant
            // Secondary@-1
            set.ExceptWith(set); // Noncompliant
            // Secondary@-1
            set.SymmetricExceptWith(set); // Noncompliant
            // Secondary@-1
        }

        void AlwaysTrue()
        {
            var list = new List<int>();
            var set = new HashSet<int>();

            list.SequenceEqual(list); // Noncompliant {{Change one instance of 'list' to a different value; Comparing to itself always returns true.}}
            // Secondary@-1
            Enumerable.SequenceEqual(list, list); // Noncompliant
            // Secondary@-1
            set.IsSubsetOf(set); // Noncompliant
            // Secondary@-1
            set.IsSupersetOf(set); // Noncompliant
            // Secondary@-1
            set.Overlaps(set); // Noncompliant
            // Secondary@-1
            set.SetEquals(set); // Noncompliant
            // Secondary@-1
        }

        void AlwaysFalse()
        {
            var set = new HashSet<int>();

            set.IsProperSubsetOf(set); // Noncompliant {{Change one instance of 'set' to a different value; Comparing to itself always returns false.}}
            // Secondary@-1
            set.IsProperSupersetOf(set); // Noncompliant
            // Secondary@-1
        }

        void ValidCases()
        {
            var list1 = new List<int>();
            var list2 = new List<int>();
            var set1 = new HashSet<int>();
            var set2 = new HashSet<int>();

            list1.AddRange(list2);
            list1.Concat(list2);
            Enumerable.Concat(list1, list2);

            list1.Union(list2);
            list1.Union(list2, this);
            Enumerable.Union(list1, list2);
            list1.Intersect(list2);
            Enumerable.Intersect(list1, list2);
            set1.UnionWith(set2);

            list1.SequenceEqual(list2);
            Enumerable.SequenceEqual(list1, list2);

            list1.Except(list2);
            Enumerable.Except(list1, list2);
            set1.ExceptWith(set2);
        }
    }
}
