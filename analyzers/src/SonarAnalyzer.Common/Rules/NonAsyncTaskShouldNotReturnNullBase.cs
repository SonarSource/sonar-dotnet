/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class NonAsyncTaskShouldNotReturnNullBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4586";

        private static readonly ImmutableArray<KnownType> TaskTypes =
            ImmutableArray.Create(
                KnownType.System_Threading_Tasks_Task,
                KnownType.System_Threading_Tasks_Task_T
            );

        protected static bool IsInvalidEnclosingSymbolContext(SyntaxNode enclosingMember, SemanticModel model)
        {
            var enclosingMemberSymbol = model.GetDeclaredSymbol(enclosingMember) ?? model.GetSymbolInfo(enclosingMember).Symbol;
            var enclosingMemberMethodSymbol = enclosingMemberSymbol as IMethodSymbol;

            return enclosingMemberSymbol != null
                && IsTaskReturnType(enclosingMemberSymbol, enclosingMemberMethodSymbol)
                && !IsSafeTaskReturnType(enclosingMemberMethodSymbol);
        }

        private static bool IsTaskReturnType(ISymbol symbol, IMethodSymbol methodSymbol)
        {
            return GetReturnType() is INamedTypeSymbol namedTypeSymbol
                && namedTypeSymbol.ConstructedFrom.DerivesFromAny(TaskTypes);

            ITypeSymbol GetReturnType() =>
                methodSymbol != null
                    ? methodSymbol.ReturnType
                    : symbol.GetSymbolType();
        }

        private static bool IsSafeTaskReturnType(IMethodSymbol methodSymbol) =>
            // IMethodSymbol also handles lambdas
            methodSymbol != null
            && methodSymbol.IsAsync;
    }
}
