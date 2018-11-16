/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal static class MemberDescriptorHelper
    {
        public static bool IsMatch<TSymbolType>(this MemberDescriptor member, string identifierName,
            Lazy<TSymbolType> symbol, bool checkOverriddenMethods)
            where TSymbolType : class, ISymbol
        {
            if (identifierName == null)
            {
                return false;
            }

            if (identifierName != member.Name)
            {
                return false;
            }
            if (symbol.Value == null)
            {
                return false; // No need to continue looping if the symbol is null
            }
            var containingType = symbol.Value.ContainingType.ConstructedFrom;
            if (!checkOverriddenMethods && containingType.Is(member.ContainingType) ||
                checkOverriddenMethods && containingType.DerivesOrImplements(member.ContainingType))
            {
                return true;
            }

            return false;
        }

        public static bool IsMatch<TSymbolType>(string identifierName, Lazy<TSymbolType> symbolFetcher,
            bool checkOverriddenMethods, params MemberDescriptor[] members)
            where TSymbolType : class, ISymbol
        {
            if (identifierName == null)
            {
                return false;
            }

            foreach (var m in members)
            {
                if (identifierName != m.Name)
                {
                    continue;
                }
                if (symbolFetcher.Value == null)
                {
                    return false; // No need to continue looping if the symbol is null
                }
                var containingType = symbolFetcher.Value.ContainingType.ConstructedFrom;
                if (!checkOverriddenMethods && containingType.Is(m.ContainingType) ||
                    checkOverriddenMethods && containingType.DerivesOrImplements(m.ContainingType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
