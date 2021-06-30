/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ExpressionComplexityBase<TExpression> : ParameterLoadingDiagnosticAnalyzer
        where TExpression : SyntaxNode
    {
        protected const string DiagnosticId = "S1067";
        private const string MessageFormat = "Reduce the number of conditional operators ({1}) used in the expression (maximum allowed {0}).";
        private const int DefaultValueMaximum = 3;

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }
        protected abstract bool IsComplexityIncreasingKind(SyntaxNode node);
        protected abstract bool IsCompoundExpression(SyntaxNode node);
        protected abstract bool IsPatternRoot(SyntaxNode node);

        [RuleParameter("max", PropertyType.Integer, "Maximum number of allowed conditional operators in an expression", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ExpressionComplexityBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources, isEnabledByDefault: false);

        protected sealed override void Initialize(ParameterLoadingAnalysisContext context) =>
            context.RegisterSyntaxTreeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
                {
                    var root = c.Tree.GetRoot();
                    var rootExpressions = NoncompoundSubexpressions(root);
                    var compoundExpressionsDescendants = root.DescendantNodes().Where(IsCompoundExpression).SelectMany(NoncompoundSubexpressions);

                    foreach (var expression in rootExpressions.Concat(compoundExpressionsDescendants))
                    {
                        var complexity = expression.DescendantNodesAndSelf(x => !IsCompoundExpression(x)).Count(IsComplexityIncreasingKind);
                        if (complexity > Maximum)
                        {
                            c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, expression.GetLocation(), Maximum, complexity));
                        }
                    }
                });

        private IEnumerable<SyntaxNode> NoncompoundSubexpressions(SyntaxNode node) =>
            node.DescendantNodes(x => !IsExpressionOrPatternRoot(x) || x == node).Where(x => IsExpressionOrPatternRoot(x) && !IsCompoundExpression(x));

        private bool IsExpressionOrPatternRoot(SyntaxNode node) =>
            node is TExpression|| IsPatternRoot(node);
    }
}
