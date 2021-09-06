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
        public Bar(IList<string> list)
            : base(list)
        {
        }
    }

    public class NotPublicAndReadonly
    {
        public string[] notReadonlyStrings;
        protected readonly bool[] bools;
        private readonly int[] ints;
        internal readonly float[] floats;

        private class NotEffectivelyPublic
        {
            public readonly string[] strings;
        }
    }

    public class ImmutableUninitialized<T>
    {
        public readonly ReadOnlyCollection<string> readonlyCollectionString;
        public readonly ReadOnlyDictionary<string, string> readonlyDictionaryStrings;
        public readonly IReadOnlyList<string> iReadonlyListString;
        public readonly IReadOnlyCollection<string> iReadonlyCollectionString;
        public readonly IReadOnlyDictionary<string, string> iReadonlyDictionaryStrings;
        public readonly Bar bar;
        public readonly IImmutableDictionary<string, string> iImmutableDictionary;
        public readonly IImmutableList<string> iImmutableList;
        public readonly IImmutableQueue<string> iImmutableQueue;
        public readonly IImmutableSet<string> iImmutableSet;
        public readonly IImmutableStack<string> iImmutableStack;
        public readonly ImmutableArray<string> immutableArray;
        public readonly ImmutableSortedSet<string> immutableSortedSet;
        public static readonly ImmutableSortedSet<string> staticReadonlyImmutableSortedSet;
        public readonly ImmutableSortedSet<T> genericImmutableSortedSet;
    }

    public class MutableInitializedWithImmutable
    {
        public readonly ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");
        public readonly IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b");
        public readonly IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");
        public readonly IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>();
        public readonly IList<string> iList = null;
    }

    public class MutableInitializedWithMutable
    {
        public readonly ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" }; // Noncompliant {{Use an immutable collection or reduce the accessibility of the non-private readonly field 'isetInitializaedWithHashSet'.}}
//                      ^^^^^^^^^^^^
        public readonly IList<string> iListInitializaedWithList = new List<string> { "a", "b" }; // Noncompliant
        public readonly IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant
    }

    public class HandleFieldWithMultipleVariables
    {
        public readonly string validfield;
        public readonly ISet<string> set1 = new HashSet<string>(), set2 = new HashSet<string>(); // Noncompliant {{Use an immutable collection or reduce the accessibility of the non-private readonly fields 'set1' and 'set2'.}}
    }

    // When the types are uninitialized, this is equivalent to being initialized to null so we don't report
    public class MutableUninitialized
    {
        public readonly string[] strings;
        public readonly Array array;
        public readonly List<string> listString;
        public readonly LinkedList<string> linkedListString;
        public readonly SortedList<string, string> sortedListString;
        public readonly ObservableCollection<string> observableCollectionString;
        public readonly Foo foo;

        public readonly ICollection<string> iCollectionString;
        public readonly IList<string> iListString;
        public readonly ISet<string> set1, set2;
    }

    // Issue #1491: https://github.com/SonarSource/sonar-dotnet/issues/1491
    public class MutableInitializedWithImmutableInConstructor
    {
        public readonly string[] foo; // Compliant - set to null in ctor
        public readonly ISet<string> set; // Compliant - set to immutable in ctor

        MutableInitializedWithImmutableInConstructor()
        {
            foo = null;
            set = ImmutableHashSet.Create("a", "b");
        }
    }

    public class MutableInitializedInConstructors
    {
        public readonly ISet<string> set; // Noncompliant - one of the ctor sets a mutable type

        MutableInitializedInConstructors()
        {
            set = ImmutableHashSet.Create("a", "b");
        }

        MutableInitializedInConstructors(string s)
        {
            set = new HashSet<string>();
        }
    }
}
