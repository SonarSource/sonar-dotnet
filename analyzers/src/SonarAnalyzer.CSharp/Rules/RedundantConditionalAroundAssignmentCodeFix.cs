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

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantConditionalAroundAssignmentCodeFix : SonarCodeFix
    {
        private const string Title = "Remove redundant conditional";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantConditionalAroundAssignment.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var condition = root.FindNode(diagnosticSpan) as ExpressionSyntax;
            var ifStatement = condition?.FirstAncestorOrSelf<IfStatementSyntax>();

            if (ifStatement != null)
            {
                return HandleIfStatement(root, context, ifStatement);
            }

            var switchExpression = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<SyntaxNode>(x => x.IsKind(SyntaxKindEx.SwitchExpression));
            if (switchExpression != null)
            {
                return HandleSwitchExpression(root, context, switchExpression);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private static Task HandleIfStatement(SyntaxNode root, SonarCodeFixContext context, IfStatementSyntax ifStatement)
        {
            var statement = ifStatement.Statement;
            if (statement is BlockSyntax block)
            {
                statement = block.Statements.FirstOrDefault();
            }

            if (statement == null)
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = root.ReplaceNode(
                        ifStatement,
                        statement.WithTriviaFrom(ifStatement));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static Task HandleSwitchExpression(SyntaxNode root, SonarCodeFixContext context, SyntaxNode switchExpression)
        {
            var switchArm = ((SwitchExpressionSyntaxWrapper)switchExpression).Arms.FirstOrDefault();
            if (switchArm.SyntaxNode == null || switchArm.SyntaxNode.Parent.ChildNodes().Count(x => x.IsKind(SyntaxKindEx.SwitchExpressionArm)) != 1)
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = root.ReplaceNode(
                        switchExpression,
                        switchArm.Expression.WithTriviaFrom(switchExpression));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
