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
    public abstract class ExpressionComplexityBase<TSyntaxKind> : ParametrizedDiagnosticAnalyzer
        where TSyntaxKind : struct, Enum
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

        protected sealed override void Initialize(SonarParametrizedAnalysisContext context) =>
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
                {
                    if (IsRoot(c.Node))
                    {
                        var complexity = CalculateComplexity(c.Node);
                        if (complexity > Maximum)
                        {
                            c.ReportIssue(CreateDiagnostic(rule, c.Node.GetLocation(), Maximum, complexity));
                        }
                    }
                }, ComplexityIncreasingKinds.Concat(TransparentKinds).ToArray());

        private bool IsRoot(SyntaxNode node) =>
            node?.Parent == null
            || (node.Parent.Kind<TSyntaxKind>() is var parentKind && !ComplexityIncreasingKinds.Contains(parentKind) && !TransparentKinds.Contains(parentKind));

        private int CalculateComplexity(SyntaxNode node)
        {
            var complexity = 0;
            Stack<SyntaxNode> stack = new();

            stack.Push(node);
            while (stack.TryPop(out var current))
            {
                if (ComplexityIncreasingKinds.Contains(current.Kind<TSyntaxKind>()))
                {
                    complexity++;
                }
                stack.Push(ExpressionChildren(current));
            }

            return complexity;
        }
    }
}
