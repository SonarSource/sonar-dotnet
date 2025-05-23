﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class EmptyMethodCodeFix : SonarCodeFix
    {
        internal const string TitleThrow = "Throw NotSupportedException";
        internal const string TitleComment = "Add comment";

        private const string LiteralNotSupportedException = "NotSupportedException";
        private const string LiteralSystem = "System";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EmptyMethod.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            if (root.FindNode(context.Diagnostics.First().Location.SourceSpan, getInnermostNodeForTie: true)
                .FirstAncestorOrSelf<SyntaxNode>(x => x.IsAnyKind(EmptyMethod.SupportedSyntaxKinds))
                .GetBody() is { CloseBraceToken.IsMissing: false, OpenBraceToken.IsMissing: false } body)
            {
                await RegisterCodeFixesForMethodsAsync(context, root, body).ConfigureAwait(false);
            }
        }

        private static async Task RegisterCodeFixesForMethodsAsync(SonarCodeFixContext context, SyntaxNode root, BlockSyntax methodBody)
        {
            context.RegisterCodeFix(
                TitleComment,
                c =>
                {
                    var newMethodBody = methodBody;
                    newMethodBody = newMethodBody
                        .WithOpenBraceToken(newMethodBody.OpenBraceToken
                            .WithTrailingTrivia(SyntaxFactory.TriviaList()
                                .Add(SyntaxFactory.EndOfLine(Environment.NewLine))));

                    newMethodBody = newMethodBody
                        .WithCloseBraceToken(newMethodBody.CloseBraceToken
                            .WithLeadingTrivia(SyntaxFactory.TriviaList()
                                .Add(SyntaxFactory.Comment("// Method intentionally left empty."))
                                .Add(SyntaxFactory.EndOfLine(Environment.NewLine))));

                    var newRoot = methodBody.Parent is AccessorDeclarationSyntax accessor
                        ? root.ReplaceNode(
                            accessor,
                            accessor.WithBody(newMethodBody.WithTriviaFrom(accessor.Body)).WithAdditionalAnnotations(Formatter.Annotation))
                        : root.ReplaceNode(
                            methodBody,
                            newMethodBody.WithTriviaFrom(methodBody).WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var systemNeedsToBeAdded = NamespaceNeedsToBeAdded(methodBody, semanticModel);

            var memberAccessRoot = systemNeedsToBeAdded
                ? (NameSyntax)SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName(LiteralSystem),
                        SyntaxFactory.IdentifierName(LiteralNotSupportedException))
                : SyntaxFactory.IdentifierName(LiteralNotSupportedException);

            context.RegisterCodeFix(
                TitleThrow,
                c =>
                {
                    var newRoot = root.ReplaceNode(methodBody,
                        methodBody.WithStatements(
                            SyntaxFactory.List(
                                new StatementSyntax[]
                                {
                                    SyntaxFactory.ThrowStatement(
                                        SyntaxFactory.ObjectCreationExpression(
                                            memberAccessRoot,
                                            SyntaxFactory.ArgumentList(),
                                            null))
                                }))
                                .WithTriviaFrom(methodBody)
                                .WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static bool NamespaceNeedsToBeAdded(BlockSyntax methodBody, SemanticModel semanticModel) =>
            semanticModel.LookupNamespacesAndTypes(methodBody.CloseBraceToken.SpanStart)
                .All(x => x is not INamedTypeSymbol { IsType: true, Name: LiteralNotSupportedException, ContainingNamespace.Name: LiteralSystem });
    }
}
