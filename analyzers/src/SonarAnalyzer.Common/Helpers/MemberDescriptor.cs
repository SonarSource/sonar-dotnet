/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
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
            $"{ContainingType.ShortName}.{Name}";

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
            && members.Any(x => memberName.Equals(x.Name, nameComparison))
            && memberSymbol.Value is { } symbol
            && members.Any(x => x.HasSameContainingType(symbol, checkOverriddenMethods));

        private static bool HasSameName(string name1, string name2, StringComparison comparison) =>
            name1 != null && name1.Equals(name2, comparison);

        private bool HasSameContainingType<TSymbolType>(TSymbolType memberSymbol, bool checkOverriddenMethods)
            where TSymbolType : class, ISymbol
        {
            var containingType = memberSymbol.ContainingType?.ConstructedFrom;
            return checkOverriddenMethods ? containingType.DerivesOrImplements(ContainingType) : containingType.Is(ContainingType);
        }
    }
}
