/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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


using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    public abstract class EquatableRuleBase : SonarDiagnosticAnalyzer
    {
        protected const string EqualsMethodName = nameof(object.Equals);

        protected static bool IsCompilableIEquatableTSymbol(INamedTypeSymbol namedTypeSymbol)
        {
            return namedTypeSymbol.ConstructedFrom.Is(KnownType.System_IEquatable_T) &&
                namedTypeSymbol.TypeArguments.Length == 1;
        }

        protected static bool IsIEquatableEqualsMethodCandidate(IMethodSymbol methodSymbol)
        {
            return methodSymbol.MethodKind == MethodKind.Ordinary &&
                methodSymbol.Name == EqualsMethodName &&
                !methodSymbol.IsOverride &&
                methodSymbol.ReturnType.Is(KnownType.System_Boolean) &&
                methodSymbol.Parameters.Length == 1;
        }
    }
}
