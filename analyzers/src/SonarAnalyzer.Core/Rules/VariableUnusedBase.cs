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

namespace SonarAnalyzer.Core.Rules;

public abstract class VariableUnusedBase : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S1481";
    protected const string MessageFormat = "Remove the unused local variable '{0}'.";

    internal static IEnumerable<ISymbol> GetUsedSymbols(SyntaxNode node, SemanticModel model)
    {
        var symbolInfo = model.GetSymbolInfo(node);
        if (symbolInfo.Symbol is not null)
        {
            yield return symbolInfo.Symbol;
        }

        foreach (var candidate in symbolInfo.CandidateSymbols)
        {
            yield return candidate;
        }
    }

    protected abstract class UnusedLocalsCollectorBase<TLocalDeclaration>
        where TLocalDeclaration : SyntaxNode
    {
        private readonly ISet<ISymbol> declaredLocals = new HashSet<ISymbol>();
        private readonly ISet<ISymbol> usedLocals = new HashSet<ISymbol>();

        protected abstract IEnumerable<SyntaxNode> GetDeclaredVariables(TLocalDeclaration localDeclaration);

        public void CollectDeclarations(SonarSyntaxNodeReportingContext c) =>
            declaredLocals.UnionWith(
                GetDeclaredVariables((TLocalDeclaration)c.Node)
                    .Select(x => c.Model.GetDeclaredSymbol(x) ?? c.Model.GetSymbolInfo(x).Symbol)
                    .WhereNotNull());

        public void CollectUsages(SonarSyntaxNodeReportingContext c) =>
            usedLocals.UnionWith(GetUsedSymbols(c.Node, c.Model));

        public void ReportUnusedVariables(SonarCodeBlockReportingContext c, DiagnosticDescriptor rule)
        {
            foreach (var unused in declaredLocals.Except(usedLocals))
            {
                c.ReportIssue(rule, unused.Locations.First(), unused.Name);
            }
        }
    }
}
