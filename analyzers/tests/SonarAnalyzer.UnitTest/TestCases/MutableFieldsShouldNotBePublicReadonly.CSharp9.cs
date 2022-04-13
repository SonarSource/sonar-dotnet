using System;
using System.Collections;
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
        public readonly ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" };                          // Noncompliant
        public readonly IList<string> iListInitializaedWithList = new List<string> { "a", "b" };                              // Noncompliant
        public readonly IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant
    }

    public record MutableInitializedWithMutablePositional(string Property)
    {
        public readonly ISet<string> isetInitializaedWithHashSet = new HashSet<string> { "a", "b" };                          // Noncompliant
                                                                                                                              // Noncompliant@-1 FP Duplicate. see MutableFieldsShouldNotBePublicStatic.CSharp9.cs for details
        public readonly IList<string> iListInitializaedWithList = new List<string> { "a", "b" };                              // Noncompliant
                                                                                                                              // Noncompliant@-1 FP Duplicate
        public readonly IDictionary<string, string> iDictionaryInitializaedWithDictionary = new Dictionary<string, string>(); // Noncompliant
                                                                                                                              // Noncompliant@-1 FP Duplicate
    }
}
