using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Collections.Specialized;

namespace Tests.Diagnostics
{
    public record WhenNonReadonlyAlwaysReport
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");   // Noncompliant {{Use an immutable collection or reduce the accessibility of the public static field 'iSetInitializaedWithImmutableSet'.}}
        //            ^^^^^^^^^^^^
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");   // Noncompliant
        public static IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>(); // Noncompliant
        public static IList<string> iList = null;                                                                                                // Noncompliant
        public static ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" };                                               // Noncompliant
        public static IList<string> iListInitializaedWithList = new List<string> { "a", "b" };                                                   // Noncompliant
        public static IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>();                      // Noncompliant
        public static string[] strings;                                        // Noncompliant
        public static Array array;                                             // Noncompliant
        public static List<string> listString;                                 // Noncompliant
        public static LinkedList<string> linkedListString;                     // Noncompliant
        public static SortedList<string, string> sortedListString;             // Noncompliant
        public static ObservableCollection<string> observableCollectionString; // Noncompliant
        public static ICollection<string> iCollectionString;                   // Noncompliant
        public static IList<string> iListString;                               // Noncompliant
    }

    public record WhenReadonlyAndInitializedToImmutable
    {
        public static readonly ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b");
        public static readonly IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");
        public static readonly IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>();
        public static readonly IList<string> iList = null;
    }

    public record WhenNonReadonlyAlwaysReportPositional(string Property, int Value)
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");   // Noncompliant
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");   // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8638
namespace Repro_8638
{
    public class FrozenCollections
    {
        public static FrozenDictionary<string, string> FrozenDictionaryTest = new Dictionary<string, string>().ToFrozenDictionary(); // Compilant
        public static FrozenSet<string> FrozenSetTest = new HashSet<string>().ToFrozenSet(); // Compilant
    }
}

namespace CSharp10
{
    public record struct WhenNonReadonlyAlwaysReport
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");   // Noncompliant {{Use an immutable collection or reduce the accessibility of the public static field 'iSetInitializaedWithImmutableSet'.}}
        //            ^^^^^^^^^^^^
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");   // Noncompliant
        public static IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>(); // Noncompliant
        public static IList<string> iList = null;                                                                                                // Noncompliant
        public static ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" };                                               // Noncompliant
        public static IList<string> iListInitializaedWithList = new List<string> { "a", "b" };                                                   // Noncompliant
        public static IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>();                      // Noncompliant
        public static string[] strings;                                        // Noncompliant
        public static Array array;                                             // Noncompliant
        public static List<string> listString;                                 // Noncompliant
        public static LinkedList<string> linkedListString;                     // Noncompliant
        public static SortedList<string, string> sortedListString;             // Noncompliant
        public static ObservableCollection<string> observableCollectionString; // Noncompliant
        public static ICollection<string> iCollectionString;                   // Noncompliant
        public static IList<string> iListString;                               // Noncompliant
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
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");   // Noncompliant
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");   // Noncompliant
    }

    public struct WhenNonReadonlyAlwaysReportStruct
    {
        public static ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");   // Noncompliant
        public static IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b"); // Noncompliant
        public static IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");   // Noncompliant
    }
}

namespace CSharp13
{
    public class OrderedDictionary
    {
        public static OrderedDictionary<string, string> orderedDictionary; // Noncompliant
        public static OrderedDictionary<string, string> a, b;              // Noncompliant
        public static readonly OrderedDictionary<string, string> orderedDictionary2;
        public static readonly OrderedDictionary<string, string> a2, b2;
    }

    // https://sonarsource.atlassian.net/browse/NET-389
    public class ReadOnlySet
    {
        public static ReadOnlySet<string> readonlySet;                     // Compliant
        public static readonly ReadOnlySet<string> readonlySet2;
    }
}
