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

using SonarAnalyzer.Common;

namespace SonarAnalyzer.Rules
{
    public abstract class CognitiveComplexityBase<TSyntaxKind> : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S3776";
        private const string MessageFormat = "Refactor this {0} to reduce its Cognitive Complexity from {1} to the {2} allowed.";
        private const int DefaultThreshold = 15;
        private const int DefaultPropertyThreshold = 3;

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        [RuleParameter("threshold", PropertyType.Integer, "The maximum authorized complexity.", DefaultThreshold)]
        public int Threshold { get; set; } = DefaultThreshold;

        [RuleParameter("propertyThreshold", PropertyType.Integer, "The maximum authorized complexity in a property.", DefaultPropertyThreshold)]
        public int PropertyThreshold { get; set; } = DefaultPropertyThreshold;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected CognitiveComplexityBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        protected void CheckComplexity<TSyntax>(SonarSyntaxNodeReportingContext context,
                                                Func<TSyntax, SyntaxNode> nodeSelector,
                                                Func<TSyntax, Location> getLocationToReport,
                                                Func<SyntaxNode, CognitiveComplexity> getComplexity,
                                                string declarationType,
                                                int threshold)
            where TSyntax : SyntaxNode
        {
            var syntax = (TSyntax)context.Node;
            var nodeToAnalyze = nodeSelector(syntax);
            if (nodeToAnalyze == null)
            {
                return;
            }

            var metric = getComplexity(nodeToAnalyze);

            if (metric.Complexity > Threshold)
            {
                context.ReportIssue(rule, getLocationToReport(syntax), metric.Locations, declarationType, metric.Complexity.ToString(), threshold.ToString());
            }
        }
    }
}
