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

using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    [System.Diagnostics.DebuggerStepThrough]
    public class MethodSignature
    {
        internal MethodSignature(KnownType containingType, string name)
        {
            ContainingType = containingType;
            Name = name;
        }

        internal KnownType ContainingType { get; }
        internal string Name { get; }

        internal string ToShortName()
        {
            var containingTypeName = ContainingType.TypeName.Split('.').Last();
            return string.Concat(containingTypeName, ".", Name);
        }

        internal string ToFullName()
        {
            return string.Concat(ContainingType.TypeName, ".", Name);
        }

        internal bool Is(ITypeSymbol knownType, string methodName)
        {
            return knownType != null &&
                methodName != null &&
                knownType.Is(ContainingType) &&
                methodName == Name;
        }
    }
}
