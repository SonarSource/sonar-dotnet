﻿/*
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

using System.Globalization;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ForLoopCounterCondition : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1994";
        private const string MessageFormat = "{0}";
        private const string MessageFormatNotEmpty = "This loop's stop condition tests {0} but the incrementer updates {1}.";
        private const string MessageFormatEmpty = "This loop's stop incrementer updates {0} but the stop condition doesn't test any variables.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;

                    var incrementedSymbols = GetIncrementorSymbols(forNode, c.Model).ToList();
                    if (!incrementedSymbols.Any())
                    {
                        return;
                    }

                    var conditionSymbols = GetReadSymbolsCondition(forNode, c.Model).ToList();
                    if (conditionSymbols.Intersect(incrementedSymbols).Any())
                    {
                        return;
                    }

                    var incrementedVariables = incrementedSymbols.Select(s => $"'{s.Name}'").OrderBy(s => s).JoinAnd();
                    if (conditionSymbols.Any())
                    {
                        var conditionVariables = conditionSymbols.Select(s => $"'{s.Name}'").OrderBy(s => s).JoinAnd();
                        c.ReportIssue(Rule, forNode.Condition, string.Format(CultureInfo.InvariantCulture, MessageFormatNotEmpty, conditionVariables, incrementedVariables));
                    }
                    else
                    {
                        c.ReportIssue(Rule, forNode.ForKeyword, string.Format(CultureInfo.InvariantCulture, MessageFormatEmpty, incrementedVariables));
                    }
                },
                SyntaxKind.ForStatement);

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

        private static IEnumerable<ISymbol> GetReadSymbolsCondition(ForStatementSyntax forNode, SemanticModel semanticModel)
        {
            if (forNode.Condition == null)
            {
                return Array.Empty<ISymbol>();
            }

            var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(forNode.Condition);

            return dataFlowAnalysis.Succeeded
                ? dataFlowAnalysis.ReadInside.Distinct()
                : Array.Empty<ISymbol>();
        }
    }
}
