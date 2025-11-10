/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Rules
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
