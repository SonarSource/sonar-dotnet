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
    public abstract class FunctionComplexityBase : ParametrizedDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S1541";
        protected const string MessageFormat = "The Cyclomatic Complexity of this {2} is {1} which is greater than {0} authorized.";

        protected const int DefaultValueMaximum = 10;

        [RuleParameter("maximumFunctionComplexityThreshold", PropertyType.Integer, "The maximum authorized complexity.", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected abstract override void Initialize(SonarParametrizedAnalysisContext context);

        protected void CheckComplexity<TSyntax>(SonarSyntaxNodeReportingContext context, Func<TSyntax, SyntaxNode> nodeSelector, Func<TSyntax, Location> location,
            string declarationType)
            where TSyntax : SyntaxNode
        {
            var syntax = (TSyntax)context.Node;
            var nodeToAnalyze = nodeSelector(syntax);
            if (nodeToAnalyze == null)
            {
                return;
            }

            var complexity = GetComplexity(nodeToAnalyze, context.SemanticModel);
            if (complexity > Maximum)
            {
                context.ReportIssue(CreateDiagnostic(SupportedDiagnostics[0], location(syntax), Maximum, complexity, declarationType));
            }
        }

        protected void CheckComplexity<TSyntax>(SonarSyntaxNodeReportingContext context, Func<TSyntax, Location> location,
            string declarationType)
            where TSyntax : SyntaxNode
        {
            CheckComplexity(context, t => t, location, declarationType);
        }

        protected abstract int GetComplexity(SyntaxNode node, SemanticModel semanticModel);
    }
}
