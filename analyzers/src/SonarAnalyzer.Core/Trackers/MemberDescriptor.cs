/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Trackers;

public class MemberDescriptor
{
    internal KnownType ContainingType { get; }
    internal string Name { get; }

    internal MemberDescriptor(KnownType containingType, string name)
    {
        ContainingType = containingType;
        Name = name;
    }

    public override string ToString() =>
        $"{ContainingType.TypeName}.{Name}";

    public bool IsMatch(string memberName, ITypeSymbol containingType, StringComparison nameComparison) =>
        HasSameName(memberName, Name, nameComparison)
        && containingType.Is(ContainingType);

    public bool IsMatch<TSymbolType>(string memberName, Lazy<TSymbolType> memberSymbol, StringComparison nameComparison)
        where TSymbolType : class, ISymbol =>
        HasSameName(memberName, Name, nameComparison)
        && memberSymbol.Value is { } symbol
        && HasSameContainingType(symbol, checkOverriddenMethods: true);

    public static bool MatchesAny<TSymbolType>(string memberName, Lazy<TSymbolType> memberSymbol, bool checkOverriddenMethods, StringComparison nameComparison, params MemberDescriptor[] members)
        where TSymbolType : class, ISymbol =>
        memberName != null
        // avoid calling the semantic model if no name matches
        && members.Any(x => memberName.Equals(x.Name, nameComparison))
        && memberSymbol.Value is { } symbol
        // we need to check both Name and Type to make sure the right method on the right type is called
        && members.Any(x => memberName.Equals(x.Name, nameComparison) && x.HasSameContainingType(symbol, checkOverriddenMethods));

    private static bool HasSameName(string name1, string name2, StringComparison comparison) =>
        name1 != null && name1.Equals(name2, comparison);

    private bool HasSameContainingType<TSymbolType>(TSymbolType memberSymbol, bool checkOverriddenMethods)
        where TSymbolType : class, ISymbol
    {
        var containingType = memberSymbol.ContainingType?.ConstructedFrom;
        return containingType != null && checkOverriddenMethods ? containingType.DerivesOrImplements(ContainingType) : containingType.Is(ContainingType);
    }
}
