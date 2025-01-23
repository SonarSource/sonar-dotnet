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

            var complexity = GetComplexity(nodeToAnalyze, context.Model);
            if (complexity > Maximum)
            {
                context.ReportIssue(SupportedDiagnostics[0], location(syntax), Maximum.ToString(), complexity.ToString(), declarationType);
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
