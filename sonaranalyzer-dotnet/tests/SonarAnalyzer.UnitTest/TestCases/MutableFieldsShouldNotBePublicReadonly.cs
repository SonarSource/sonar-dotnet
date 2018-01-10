using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Tests.Diagnostics
{
    class Foo : ICollection<string> { }
    class Bar : ReadOnlyCollection<string> { }

    class CompliantCases<T>
    {
        protected readonly bool[] bools; // Compliant
        private readonly int[] ints; // Compliant
        internal readonly float[] floats; // Compliant
        public readonly ReadOnlyCollection<string> readonlyCollectionString; // Compliant
        public readonly ReadOnlyDictionary<string, string> readonlyDictionaryStrings; // Compliant
        public readonly IReadOnlyList<string> iReadonlyListString; // Compliant
        public readonly IReadOnlyCollection<string> iReadonlyCollectionString; // Compliant
        public readonly IReadOnlyDictionary<string, string> iReadonlyDictionaryStrings; // Compliant
        public string[] notReadonlyStrings; // Compliant
        public readonly Bar bar; // Compliant
        public readonly IImmutableDictionary<string, string> iImmutableDictionary; // Compliant
        public readonly IImmutableList<string> iImmutableList; // Compliant
        public readonly IImmutableQueue<string> iImmutableQueue; // Compliant
        public readonly IImmutableSet<string> iImmutableSet; // Compliant
        public readonly IImmutableStack<string> iImmutableStack; // Compliant
        public readonly ImmutableArray<string> immutableArray; // Compliant
        public readonly ImmutableSortedSet<string> immutableSortedSet; // Compliant
        public static readonly ImmutableSortedSet<string> staticReadonlyImmutableSortedSet; // Compliant

        public readonly ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");
        public readonly IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b");
        public readonly IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");
        public readonly IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>();
    }

    class GenericCompliantCases<T>
    {
        public readonly ImmutableSortedSet<T> genericImmutableSortedSet; // Compliant
    }

    class InvalidCases
    {
        public readonly string[] strings; // Noncompliant {{Use an immutable collection or reduce the accessibility of this field.}}
//                               ^^^^^^^
        public readonly Array array; // Noncompliant
        public readonly ICollection<string> iCollectionString; // Noncompliant
        public readonly IList<string> iListString; // Noncompliant
        public readonly List<string> listString; // Noncompliant
        public readonly LinkedList<string> linkedListString; // Noncompliant
        public readonly SortedList<string, string> sortedListString; // Noncompliant
        public readonly ObservableCollection<string> observableCollectionString; // Noncompliant
        public readonly Foo foo; // Noncompliant

        public readonly ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" }; // Noncompliant
        public readonly IList<string> iListInitializaedWithList = new List<string> { "a", "b" }; // Noncompliant
        public readonly IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant
    }
}