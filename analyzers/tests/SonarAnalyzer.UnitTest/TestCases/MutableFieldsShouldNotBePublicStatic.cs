using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Tests.Diagnostics
{
    public class Foo : ICollection<string>
    {
        public int Count { get; }
        public bool IsReadOnly { get; }

        public void Add(string item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(string item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(string item)
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }

    public class Bar : ReadOnlyCollection<string>
    {
        public Bar(IList<string> list) : base(list)
        {
        }
    }

    public class NotPublicAndStatic
    {
        public string[] notReadonlyStrings;
        protected static bool[] bools;
        private static int[] ints;
        internal static float[] floats;

        private class NotEffectivelyPublic
        {
            public static string[] strings;
        }
    }

    public class ImmutableUninitialized<T>
    {
        public static ReadOnlyCollection<string> readonlyCollectionString;
        public static ReadOnlyDictionary<string, string> readonlyDictionaryStrings;
        public static IReadOnlyList<string> iReadonlyListString;
        public static IReadOnlyCollection<string> iReadonlyCollectionString;
        public static IReadOnlyDictionary<string, string> iReadonlyDictionaryStrings;
        public static Bar bar;
        public static IImmutableDictionary<string, string> iImmutableDictionary;
        public static IImmutableList<string> iImmutableList;
        public static IImmutableQueue<string> iImmutableQueue;
        public static IImmutableSet<string> iImmutableSet;
        public static IImmutableStack<string> iImmutableStack;
        public static ImmutableArray<string> immutableArray;
        public static ImmutableSortedSet<string> immutableSortedSet;
        public static ImmutableSortedSet<T> genericImmutableSortedSet;
        public static readonly ImmutableSortedSet<string> staticReadonlyImmutableSortedSet;
    }

    public class WhenNonReadonlyAlwaysReport
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b"); // Noncompliant {{Use an immutable collection or reduce the accessibility of the public static field 'iSetInitializaedWithImmutableSet'.}}
//                    ^^^^^^^^^^^^
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b"); // Noncompliant
        public static IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>(); // Noncompliant
        public static IList<string> iList = null; // Noncompliant
        public static ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" }; // Noncompliant
        public static IList<string> iListInitializaedWithList = new List<string> { "a", "b" }; // Noncompliant
        public static IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant
        public static string[] strings; // Noncompliant
        public static Array array; // Noncompliant
        public static List<string> listString; // Noncompliant
        public static LinkedList<string> linkedListString; // Noncompliant
        public static SortedList<string, string> sortedListString; // Noncompliant
        public static ObservableCollection<string> observableCollectionString; // Noncompliant
        public static Foo foo; // Noncompliant
        public static ICollection<string> iCollectionString; // Noncompliant
        public static IList<string> iListString; // Noncompliant
    }

    public class HandleFieldWithMultipleVariables
    {
        public static string validfield;
        public static ISet<string> set1, set2; // Noncompliant {{Use an immutable collection or reduce the accessibility of the public static fields 'set1' and 'set2'.}}
    }

    public class WhenReadonlyAndInitializedToImmutable
    {
        public static readonly ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");
        public static readonly IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>();
        public static readonly IList<string> iList = null;
    }

    public class WhenReadonlyAndInitializedToImmutableInConstructor
    {
        public static readonly string[] foo; // Compliant - set to null in ctor
        public static readonly ISet<string> set; // Compliant - set to immutable in ctor

        static WhenReadonlyAndInitializedToImmutableInConstructor()
        {
            foo = null;
            set = ImmutableHashSet.Create("a", "b");
        }
    }
}
