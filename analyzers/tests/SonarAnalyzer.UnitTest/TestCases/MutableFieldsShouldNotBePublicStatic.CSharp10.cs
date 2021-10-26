using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Tests.Diagnostics
{
    public record struct WhenNonReadonlyAlwaysReport
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b"); // FN
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // FN
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b"); // FN
        public static IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>(); // FN
        public static IList<string> iList = null; // FN
        public static ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" }; // FN
        public static IList<string> iListInitializaedWithList = new List<string> { "a", "b" }; // FN
        public static IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // FN
        public static string[] strings; // FN
        public static Array array; // FN
        public static List<string> listString; // FN
        public static LinkedList<string> linkedListString; // FN
        public static SortedList<string, string> sortedListString; // FN
        public static ObservableCollection<string> observableCollectionString; // FN
        public static ICollection<string> iCollectionString; // FN
        public static IList<string> iListString; // FN
    }

    public record struct WhenReadonlyAndInitializedToImmutable
    {
        public static readonly ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");
        public static readonly IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>();
        public static readonly IList<string> iList = null;
    }

    public record struct WhenNonReadonlyAlwaysReportPositional(string Property)
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b"); // FN
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // FN
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b"); // FN
    }

    public struct WhenNonReadonlyAlwaysReportStruct
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b"); // Noncompliant
    }
}
