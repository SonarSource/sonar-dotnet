/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
                context.ReportIssue(
                    CreateDiagnostic(rule,
                                      getLocationToReport(syntax),
                                      metric.Locations.ToAdditionalLocations(),
                                      metric.Locations.ToProperties(),
                                      new object[] { declarationType, metric.Complexity, threshold }));
            }
        }
    }
}
