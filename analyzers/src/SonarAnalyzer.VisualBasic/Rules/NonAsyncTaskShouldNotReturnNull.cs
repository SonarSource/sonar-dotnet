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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class NonAsyncTaskShouldNotReturnNull : NonAsyncTaskShouldNotReturnNullBase
    {
        private const string MessageFormat = "Do not return null from this method, instead return " +
            "'Task.FromResult(Of T)(Nothing)', 'Task.CompletedTask' or 'Task.Delay(0)'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var nullLiteral = (LiteralExpressionSyntax)c.Node;

                    if (!IsParentReturnOrReturnTernary(nullLiteral))
                    {
                        return;
                    }

                    var enclosingMember = GetEnclosingMember(nullLiteral);
                    if (enclosingMember != null &&
                        !enclosingMember.IsKind(SyntaxKind.VariableDeclarator) &&
                        IsInvalidEnclosingSymbolContext(enclosingMember, c.Model))
                    {
                        c.ReportIssue(rule, nullLiteral);
                    }
                },
                SyntaxKind.NothingLiteralExpression);
        }

        private static bool IsParentReturnOrReturnTernary(SyntaxNode node)
        {
            var parent = node.GetFirstNonParenthesizedParent();

            if (parent.IsKind(SyntaxKind.ReturnStatement))
            {
                return true;
            }
            else if (parent.IsKind(SyntaxKind.TernaryConditionalExpression))
            {
                var grandParent = parent.GetFirstNonParenthesizedParent();

                return grandParent.IsKind(SyntaxKind.ReturnStatement);
            }
            else
            {
                return false;
            }
        }

        private static SyntaxNode GetEnclosingMember(LiteralExpressionSyntax literal)
        {
            foreach (var ancestor in literal.Ancestors())
            {
                switch (ancestor.Kind())
                {
                    case SyntaxKind.MultiLineFunctionLambdaExpression:
                    case SyntaxKind.SingleLineFunctionLambdaExpression:
                        return null;

                    case SyntaxKind.VariableDeclarator:
                    case SyntaxKind.PropertyBlock:
                    case SyntaxKind.FunctionBlock:
                        return ancestor;

                    default:
                        // do nothing
                        break;
                }
            }

            return null;
        }
    }
}
