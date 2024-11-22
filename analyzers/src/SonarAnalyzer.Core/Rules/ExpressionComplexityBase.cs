/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
                            c.ReportIssue(rule, c.Node, Maximum.ToString(), complexity.ToString());
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
