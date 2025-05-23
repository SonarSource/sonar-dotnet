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

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantCastCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove redundant cast";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantCast.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            if (syntaxNode.Parent is CastExpressionSyntax castExpression)
            {
                //this is handled by IDE0004 code fix.
                return Task.CompletedTask;
            }

            var castInvocation = syntaxNode as InvocationExpressionSyntax;
            var memberAccess = syntaxNode as MemberAccessExpressionSyntax;
            if (castInvocation != null ||
                memberAccess != null)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var newRoot = RemoveCall(root, castInvocation, memberAccess);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            if (syntaxNode is BinaryExpressionSyntax asExpression)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(
                            asExpression,
                            asExpression.Left.WithTriviaFrom(asExpression));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        private static SyntaxNode RemoveCall(SyntaxNode root,
            InvocationExpressionSyntax castInvocation, MemberAccessExpressionSyntax memberAccess)
        {
            return castInvocation != null
                ? RemoveExtensionMethodCall(root, castInvocation)
                : RemoveStaticMemberCall(root, memberAccess);
        }

        private static SyntaxNode RemoveStaticMemberCall(SyntaxNode root,
            MemberAccessExpressionSyntax memberAccess)
        {
            var invocation = (InvocationExpressionSyntax)memberAccess.Parent;
            return root.ReplaceNode(invocation, invocation.ArgumentList.Arguments.First().Expression);
        }

        private static SyntaxNode RemoveExtensionMethodCall(SyntaxNode root, InvocationExpressionSyntax invocation)
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            return root.ReplaceNode(invocation, memberAccess.Expression);
        }
    }
}
