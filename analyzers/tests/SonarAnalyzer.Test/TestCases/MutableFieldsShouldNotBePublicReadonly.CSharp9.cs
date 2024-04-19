using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;

namespace Tests.Diagnostics
{
    public record MutableInitializedWithImmutable
    {
        public readonly ISet<string> iSetInitializaedWithImmutableSet = ImmutableHashSet.Create("a", "b");
        public readonly IList<string> iListInitializaedWithImmutableArray = ImmutableArray.Create("a", "b");
        public readonly IList<string> iListInitializaedWithImmutableList = ImmutableList.Create("a", "b");
        public readonly IDictionary<string, string> iDictionaryInitializaedWithImmutableDictionary = ImmutableDictionary.Create<string, string>();
        public readonly IList<string> iList = null;
    }

    public record MutableInitializedWithMutable
    {
        public readonly ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" };                          // Noncompliant {{Use an immutable collection or reduce the accessibility of the non-private readonly field 'isetInitializaedWithHashSet'.}}
        //              ^^^^^^^^^^^^
        public readonly IList<string> iListInitializaedWithList = new List<string> { "a", "b" };                              // Noncompliant
        public readonly IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant
    }

    public record MutableInitializedWithMutablePositional(string Property)
    {
        public readonly ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" };                          // Noncompliant
        public readonly IList<string> iListInitializaedWithList = new List<string> { "a", "b" };                              // Noncompliant
        public readonly IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8638
namespace Repro_8638
{
    public class FrozenCollections
    {
        public readonly FrozenDictionary<string, string> FrozenDictionaryTest = new Dictionary<string, string>().ToFrozenDictionary(); // Compilant
        public readonly FrozenSet<string> FrozenSetTest = new HashSet<string>().ToFrozenSet(); // Compilant
    }
}
