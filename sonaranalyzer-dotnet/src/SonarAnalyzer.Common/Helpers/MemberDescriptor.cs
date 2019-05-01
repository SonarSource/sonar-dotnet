/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public class MemberDescriptor
    {
        internal MemberDescriptor(KnownType containingType, string name)
        {
            ContainingType = containingType;
            Name = name;
        }

        internal KnownType ContainingType { get; }

        internal string Name { get; }

        public bool IsMatch(string memberName, ITypeSymbol containingType, bool caseInsensitiveComparison = false) =>
            containingType.Is(ContainingType) && HasSameName(memberName, Name, caseInsensitiveComparison);

        public bool IsMatch<TSymbolType>(string memberName, Lazy<TSymbolType> memberSymbol, bool caseInsensitiveComparison = false)
            where TSymbolType : class, ISymbol
        {
            if (!HasSameName(memberName, Name, caseInsensitiveComparison) ||
                memberSymbol.Value == null)
            {
                return false;
            }

            if (HasSameContainingType(memberSymbol.Value, checkOverriddenMethods: true))
            {
                return true;
            }

            return false;
        }

        public static bool MatchesAny<TSymbolType>(string memberName, Lazy<TSymbolType> memberSymbol,
            bool checkOverriddenMethods, bool caseInsensitiveComparison, params MemberDescriptor[] members)
            where TSymbolType : class, ISymbol
        {
            if (memberName == null)
            {
                return false;
            }

            foreach (var m in members)
            {
                if (!HasSameName(memberName, m.Name, caseInsensitiveComparison))
                {
                    continue;
                }

                if (memberSymbol.Value == null)
                {
                    return false; // No need to continue looping if the symbol is null
                }

                if (m.HasSameContainingType(memberSymbol.Value, checkOverriddenMethods))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSameName(string name1, string name2, bool caseInsensitiveComparison) =>
            (name1 == name2 ||
                (caseInsensitiveComparison &&
                name1 != null &&
                name1.Equals(name2, StringComparison.OrdinalIgnoreCase)));

        private bool HasSameContainingType<TSymbolType>(TSymbolType memberSymbol, bool checkOverriddenMethods)
            where TSymbolType : class, ISymbol
        {
            var containingType = memberSymbol.ContainingType?.ConstructedFrom;
            return !checkOverriddenMethods && containingType.Is(ContainingType)
                || checkOverriddenMethods && containingType.DerivesOrImplements(ContainingType);
        }

        public override string ToString() =>
            $"{ContainingType.ShortName}.{Name}";
    }
}
