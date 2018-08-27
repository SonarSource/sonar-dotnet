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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ForLoopCounterCondition : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1994";
        private const string MessageFormat = "{0}";
        internal const string MessageFormatNotEmpty =
            "This loop's stop condition tests {0} but the incrementer updates {1}.";
        internal const string MessageFormatEmpty =
            "This loop's stop incrementer updates {0} but the stop condition doesn't test any variables.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;

                    var incrementedSymbols = GetIncrementorSymbols(forNode, c.SemanticModel).ToList();

                    if (!incrementedSymbols.Any())
                    {
                        return;
                    }

                    var conditionSymbols = GetReadSymbolsCondition(forNode, c.SemanticModel).ToList();

                    if (conditionSymbols.Intersect(incrementedSymbols).Any())
                    {
                        return;
                    }

                    var incrementedVariables = string.Join(",", incrementedSymbols
                        .Select(s => $"'{s.Name}'")
                        .OrderBy(s => s));
                    if (conditionSymbols.Any())
                    {
                        var conditionVariables = string.Join(",", conditionSymbols
                            .Select(s => $"'{s.Name}'")
                            .OrderBy(s => s));
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, forNode.Condition.GetLocation(),
                            string.Format(CultureInfo.InvariantCulture, MessageFormatNotEmpty, conditionVariables, incrementedVariables)));
                    }
                    else
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, forNode.ForKeyword.GetLocation(),
                            string.Format(CultureInfo.InvariantCulture, MessageFormatEmpty, incrementedVariables)));
                    }

                },
                SyntaxKind.ForStatement);
        }

        private static IEnumerable<ISymbol> GetIncrementorSymbols(ForStatementSyntax forNode,
            SemanticModel semanticModel)
        {
            var accessedSymbols = new List<ISymbol>();
            foreach (var dataFlowAnalysis in forNode.Incrementors
                .Select(semanticModel.AnalyzeDataFlow)
                .Where(dataFlowAnalysis => dataFlowAnalysis.Succeeded))
            {
                accessedSymbols.AddRange(dataFlowAnalysis.WrittenInside);
                accessedSymbols.AddRange(dataFlowAnalysis.ReadInside);
            }

            return accessedSymbols.Distinct();
        }

        private static IEnumerable<ISymbol> GetReadSymbolsCondition(ForStatementSyntax forNode,
            SemanticModel semanticModel)
        {
            if (forNode.Condition == null)
            {
                return new ISymbol[0];
            }

            var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(forNode.Condition);

            return dataFlowAnalysis.Succeeded
                ? dataFlowAnalysis.ReadInside.Distinct()
                : new ISymbol[0];
        }
    }
}
