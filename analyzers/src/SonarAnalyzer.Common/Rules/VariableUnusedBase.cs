﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class VariableUnusedBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1481";
        protected const string MessageFormat = "Remove the unused local variable '{0}'.";

        protected abstract class UnusedLocalsCollectorBase<TLocalDeclaration>
            where TLocalDeclaration : SyntaxNode
        {
            private readonly ISet<ISymbol> declaredLocals = new HashSet<ISymbol>();
            private readonly ISet<ISymbol> usedLocals = new HashSet<ISymbol>();

            protected abstract IEnumerable<SyntaxNode> GetDeclaredVariables(TLocalDeclaration localDeclaration);

            public void CollectDeclarations(SonarSyntaxNodeReportingContext c) =>
                declaredLocals.UnionWith(
                    GetDeclaredVariables((TLocalDeclaration)c.Node)
                        .Select(variable => c.SemanticModel.GetDeclaredSymbol(variable) ?? c.SemanticModel.GetSymbolInfo(variable).Symbol)
                        .WhereNotNull());

            public void CollectUsages(SonarSyntaxNodeReportingContext c) =>
                usedLocals.UnionWith(GetUsedSymbols(c.Node, c.SemanticModel));

            public void ReportUnusedVariables(SonarCodeBlockReportingContext c, DiagnosticDescriptor rule)
            {
                foreach (var unused in declaredLocals.Except(usedLocals))
                {
                    c.ReportIssue(rule, unused.Locations.First(), unused.Name);
                }
            }
        }

        internal static IEnumerable<ISymbol> GetUsedSymbols(SyntaxNode node, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol != null)
            {
                yield return symbolInfo.Symbol;
            }

            foreach (var candidate in symbolInfo.CandidateSymbols.WhereNotNull())
            {
                yield return candidate;
            }
        }
    }
}
