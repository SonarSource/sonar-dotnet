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
        public int;

        protected static bool[] bools; // Compliant
        private static int[] ints; // Compliant
        internal static float[] floats; // Compliant
        public static ReadOnlyCollection<string> readonlyCollectionString; // Compliant
        public static ReadOnlyDictionary<string, string> readonlyDictionaryStrings; // Compliant
        public static IReadOnlyList<string> iReadonlyListString; // Compliant
        public static IReadOnlyCollection<string> iReadonlyCollectionString; // Compliant
        public static IReadOnlyDictionary<string, string> iReadonlyDictionaryStrings; // Compliant
        public string[] notReadonlyStrings; // Compliant
        public static Bar bar; // Compliant
        public static IImmutableDictionary<string, string> iImmutableDictionary; // Compliant
        public static IImmutableList<string> iImmutableList; // Compliant
        public static IImmutableQueue<string> iImmutableQueue; // Compliant
        public static IImmutableSet<string> iImmutableSet; // Compliant
        public static IImmutableStack<string> iImmutableStack; // Compliant
        public static ImmutableArray<string> immutableArray; // Compliant
        public static ImmutableSortedSet<string> immutableSortedSet; // Compliant
        public static readonly ImmutableSortedSet<string> staticReadonlyImmutableSortedSet; // Compliant

        public static IImmutableList<string> iImmutableListWithInitialization = ImmutableList.Create("a", "b");

        public static readonly ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");
        public static readonly IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>();

        public static readonly string[] foo = null;
    }

    class GenericCompliantCases<T>
    {
        public static ImmutableSortedSet<T> genericImmutableSortedSet; // Compliant
    }

    class InvalidCases
    {
        public static string[] strings; // Noncompliant {{Use an immutable collection or reduce the accessibility of this field.}}
//                             ^^^^^^^
        public static Array array; // Noncompliant
        public static ICollection<string> iCollectionString; // Noncompliant
        public static IList<string> iListString; // Noncompliant
        public static List<string> listString; // Noncompliant
        public static LinkedList<string> linkedListString; // Noncompliant
        public static SortedList<string, string> sortedListString; // Noncompliant
        public static ObservableCollection<string> observableCollectionString; // Noncompliant
        public static Foo foo; // Noncompliant

        public static readonly ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" }; // Noncompliant
        public static readonly IList<string> iListInitializaedWithList = new List<string> { "a", "b" }; // Noncompliant
        public static readonly IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant

        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b"); // Noncompliant
        public static IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>(); // Noncompliant
    }
}