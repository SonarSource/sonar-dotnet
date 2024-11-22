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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class BooleanCheckInvertedCodeFix : SonarCodeFix
    {
        internal const string Title = "Invert 'Boolean' check";
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(BooleanCheckInverted.DiagnosticId);
            }
        }

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (!(root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is PrefixUnaryExpressionSyntax syntaxNode))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var expression = syntaxNode.Operand.RemoveParentheses();
                    var newBinary = ChangeOperator((BinaryExpressionSyntax)expression);

                    if (syntaxNode.Parent is ExpressionSyntax &&
                        !(syntaxNode.Parent is AssignmentExpressionSyntax))
                    {
                        newBinary = SyntaxFactory.ParenthesizedExpression(newBinary);
                    }

                    var newRoot = root.ReplaceNode(
                        syntaxNode,
                        newBinary.WithAdditionalAnnotations(Formatter.Annotation));

                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static ExpressionSyntax ChangeOperator(BinaryExpressionSyntax binary)
        {
            return
                SyntaxFactory.BinaryExpression(
                    OppositeExpressionKinds[binary.Kind()],
                    binary.Left,
                    binary.Right)
                .WithTriviaFrom(binary);
        }

        private static readonly Dictionary<SyntaxKind, SyntaxKind> OppositeExpressionKinds =
            new Dictionary<SyntaxKind, SyntaxKind>
            {
                {SyntaxKind.GreaterThanExpression, SyntaxKind.LessThanOrEqualExpression},
                {SyntaxKind.GreaterThanOrEqualExpression, SyntaxKind.LessThanExpression},
                {SyntaxKind.LessThanExpression, SyntaxKind.GreaterThanOrEqualExpression},
                {SyntaxKind.LessThanOrEqualExpression, SyntaxKind.GreaterThanExpression},
                {SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression},
                {SyntaxKind.NotEqualsExpression, SyntaxKind.EqualsExpression}
            };
    }
}
