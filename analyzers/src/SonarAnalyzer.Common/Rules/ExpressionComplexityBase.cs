/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.Rules
{
    public abstract class ExpressionComplexityBase<TExpression, TSyntaxKind> : ParameterLoadingDiagnosticAnalyzer where TSyntaxKind : struct
        where TExpression : SyntaxNode
    {
        protected const string DiagnosticId = "S1067";
        private const string MessageFormat = "Reduce the number of conditional operators ({1}) used in the expression (maximum allowed {0}).";
        private const int DefaultValueMaximum = 3;

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }
        protected abstract TSyntaxKind[] ComplexityIncreasingKinds { get; }
        protected abstract TSyntaxKind[] TransparentKinds { get; }
        protected abstract SyntaxNode[] ExpressionChildren(SyntaxNode node);

        [RuleParameter("max", PropertyType.Integer, "Maximum number of allowed conditional operators in an expression", DefaultValueMaximum)]
        public int Maximum { get; set; } = DefaultValueMaximum;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected ExpressionComplexityBase() =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat, isEnabledByDefault: false);

        protected sealed override void Initialize(ParameterLoadingAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
                {
                    if (c.Node.Parent?.Kind() is TSyntaxKind kind && (ComplexityIncreasingKinds.Contains(kind) || TransparentKinds.Contains(kind)))
                    {
                        // The parent of the expression is itself complexity increasing (e.g. &&) or transparent (e.g. parenthesis).
                        // We are only interested in the expression roots so we only report once per expression tree. Therefore we ignore any inner children, e.g.:
                        // a && b && c -> (Left: (Left: a, Right: b), Right: c) // We are only interested in the outer expression (the one with Right: c)
                        return;
                    }
                    var complexity = CalculateComplexity(c.Node);
                    if (complexity > Maximum)
                    {
                        c.ReportIssue(Diagnostic.Create(rule, c.Node.GetLocation(), Maximum, complexity));
                    }
                }, ComplexityIncreasingKinds.Concat(TransparentKinds).ToArray());

        private int CalculateComplexity(SyntaxNode node)
        {
            return CalculateComplexityRec(node, 0);

            int CalculateComplexityRec(SyntaxNode node, int currentComplexity)
            {
                if (node.Kind() is TSyntaxKind kind && ComplexityIncreasingKinds.Contains(kind))
                {
                    currentComplexity++;
                }
                foreach (var child in ExpressionChildren(node))
                {
                    currentComplexity = CalculateComplexityRec(child, currentComplexity);
                }

                return currentComplexity;
            }
        }
    }
}
