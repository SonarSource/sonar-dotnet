/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantModifierCodeFix : SonarCodeFix
    {
        internal const string TitleUnsafe = "Remove redundant 'unsafe' modifier";
        internal const string TitleChecked = "Remove redundant 'checked' and 'unchecked' modifier";
        internal const string TitlePartial = "Remove redundant 'partial' modifier";
        internal const string TitleSealed = "Remove redundant 'sealed' modifier";

        private static readonly SyntaxKind[] SimpleTokenKinds =
        {
            SyntaxKind.PartialKeyword,
            SyntaxKind.SealedKeyword
        };

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantModifier.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var token = root.FindToken(diagnosticSpan.Start);

            if (token.IsKind(SyntaxKind.UnsafeKeyword))
            {
                context.RegisterCodeFix(TitleUnsafe, c => ReplaceRoot(context, RemoveRedundantUnsafe(root, token)), context.Diagnostics);
            }
            else if (SimpleTokenKinds.Contains(token.Kind()))
            {
                var title = token.IsKind(SyntaxKind.PartialKeyword) ? TitlePartial : TitleSealed;
                context.RegisterCodeFix(title, c => ReplaceRoot(context, RemoveRedundantToken(root, token)), context.Diagnostics);
            }
            else if (token.Parent is CheckedStatementSyntax checkedStatement)
            {
                context.RegisterCodeFix(TitleChecked, c => ReplaceRoot(context, RemoveRedundantCheckedStatement(root, checkedStatement)), context.Diagnostics);
            }
            else if (token.Parent is CheckedExpressionSyntax checkedExpression)
            {
                context.RegisterCodeFix(TitleChecked, c => ReplaceRoot(context, RemoveRedundantCheckedExpression(root, checkedExpression)), context.Diagnostics);
            }
            return Task.CompletedTask;
        }

        private static Task<Document> ReplaceRoot(SonarCodeFixContext context, SyntaxNode newRoot) =>
            Task.FromResult(context.Document.WithSyntaxRoot(newRoot));

        private static SyntaxNode RemoveRedundantUnsafe(SyntaxNode root, SyntaxToken token)
        {
            if (token.Parent is UnsafeStatementSyntax unsafeStatement)
            {
                return unsafeStatement.Parent is BlockSyntax parentBlock && parentBlock.Statements.Count == 1
                    ? root.ReplaceNode(parentBlock, parentBlock.WithStatements(unsafeStatement.Block.Statements).WithAdditionalAnnotations(Formatter.Annotation))
                    : root.ReplaceNode(unsafeStatement, unsafeStatement.Block.WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                return RemoveRedundantToken(root, token);
            }
        }

        private static SyntaxNode RemoveRedundantToken(SyntaxNode root, SyntaxToken token)
        {
            var oldParent = token.Parent;
            var newParent = oldParent.ReplaceToken(token, SyntaxFactory.Token(SyntaxKind.None));
            return root.ReplaceNode(oldParent, newParent.WithLeadingTrivia(oldParent.GetLeadingTrivia()));
        }

        private static SyntaxNode RemoveRedundantCheckedStatement(SyntaxNode root, CheckedStatementSyntax checkedStatement) =>
            root.ReplaceNode(checkedStatement, SyntaxFactory.Block(checkedStatement.Block.Statements).WithTriviaFrom(checkedStatement));

        private static SyntaxNode RemoveRedundantCheckedExpression(SyntaxNode root, CheckedExpressionSyntax checkedExpression) =>
            root.ReplaceNode(checkedExpression, checkedExpression.Expression.WithTriviaFrom(checkedExpression));
    }
}
