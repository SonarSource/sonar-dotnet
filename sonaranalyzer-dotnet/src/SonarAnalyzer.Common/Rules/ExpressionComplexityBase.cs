/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ExpressionComplexityBase : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1067";
        internal const string MessageFormat = "Reduce the number of conditional operators ({1}) used in the expression (maximum allowed {0}).";

        private const int DefaultValueMaximum = 3;

        [RuleParameter("max", PropertyType.Integer,
            "Maximum number of allowed conditional operators in an expression", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;
    }

    public abstract class ExpressionComplexityBase<TExpression> : ExpressionComplexityBase
        where TExpression : SyntaxNode
    {
        public abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

        protected sealed override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxTreeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var root = c.Tree.GetRoot();

                    var rootExpressions = root
                        .DescendantNodes(node => !(node is TExpression))
                        .OfType<TExpression>()
                        .Where(expression => !IsCompoundExpression(expression));

                    var compoundExpressionsDescendants = root
                        .DescendantNodes()
                        .Where(IsCompoundExpression)
                        .SelectMany(
                            compoundExpression => compoundExpression
                                .DescendantNodes(node => compoundExpression == node || !(node is TExpression))
                                .OfType<TExpression>()
                                .Where(expression => !IsCompoundExpression(expression)));

                    var expressionsToCheck = rootExpressions.Concat(compoundExpressionsDescendants);

                    var complexExpressions = expressionsToCheck
                        .Select(expression =>
                            new
                            {
                                Expression = expression,
                                Complexity = expression
                                    .DescendantNodesAndSelf(e2 => !IsCompoundExpression(e2))
                                    .Count(IsComplexityIncreasingKind)
                            })
                        .Where(e => e.Complexity > Maximum);

                    foreach (var complexExpression in complexExpressions)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0],
                            complexExpression.Expression.GetLocation(),
                            Maximum,
                            complexExpression.Complexity));
                    }
                });
        }

        protected abstract bool IsComplexityIncreasingKind(SyntaxNode node);

        protected abstract bool IsCompoundExpression(SyntaxNode node);
    }
}
