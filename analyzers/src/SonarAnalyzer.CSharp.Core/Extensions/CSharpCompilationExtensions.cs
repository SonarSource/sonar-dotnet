/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Runtime.CompilerServices;

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class CSharpCompilationExtensions
{
    private static readonly ConditionalWeakTable<Compilation, Lazy<ImmutableDictionary<ISymbol, ImmutableArray<INamedTypeSymbol>>>> UnionCache = new();

    extension(Compilation compilation)
    {
        public bool IsCoalesceAssignmentSupported => compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp8);

        public bool IsTargetTypeConditionalSupported => compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp9);

        public bool IsLambdaDiscardParameterSupported => compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp9);

        public LanguageVersion LanguageVersion => ((CSharpCompilation)compilation).LanguageVersion;

        private ImmutableDictionary<ISymbol, ImmutableArray<INamedTypeSymbol>> UnionCaseLookup =>
            UnionCache.GetValue(compilation, x => new Lazy<ImmutableDictionary<ISymbol, ImmutableArray<INamedTypeSymbol>>>(() => BuildUnionLookup(x))).Value;

        public bool IsAtLeastLanguageVersion(LanguageVersion languageVersion) =>
            compilation.LanguageVersion.IsAtLeast(languageVersion);

        public ImmutableArray<INamedTypeSymbol> UnionsWithCaseType(ITypeSymbol caseType) =>
            compilation.UnionCaseLookup.TryGetValue(caseType.OriginalDefinition, out var unions)
                ? unions
                : ImmutableArray<INamedTypeSymbol>.Empty;

        public bool IsUnionCaseType(ITypeSymbol type) =>
            !compilation.UnionsWithCaseType(type).IsEmpty;
    }

    private static ImmutableDictionary<ISymbol, ImmutableArray<INamedTypeSymbol>> BuildUnionLookup(Compilation compilation) =>
        // Before C# 15 there are no unions, and without UnionAttribute there can be none either. In both cases we skip walking all the types.
        compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp15)
        && compilation.GetTypeByMetadataName(KnownType.System_Runtime_CompilerServices_UnionAttribute) is not null
            ? AllNamedTypes(compilation.Assembly.GlobalNamespace)
                .Where(x => x.IsUnion)
                .SelectMany(x => x.UnionCaseTypes.Select(caseType => new KeyValuePair<ISymbol, INamedTypeSymbol>(caseType.OriginalDefinition, x)))
                .GroupBy(x => x.Key, x => x.Value)
                .ToImmutableDictionary(x => x.Key, x => x.Distinct().ToImmutableArray())
            : ImmutableDictionary<ISymbol, ImmutableArray<INamedTypeSymbol>>.Empty;

    private static IEnumerable<INamedTypeSymbol> AllNamedTypes(INamespaceSymbol ns) =>
        ns.GetTypeMembers().SelectMany(AllNamedTypes)
            .Concat(ns.GetNamespaceMembers().SelectMany(AllNamedTypes));

    private static IEnumerable<INamedTypeSymbol> AllNamedTypes(INamedTypeSymbol type) =>
        new[] { type }.Concat(type.GetTypeMembers().SelectMany(AllNamedTypes));
}
