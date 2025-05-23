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
    public sealed class GenericTypeParameterEmptinessCheckingCodeFix : SonarCodeFix
    {
        internal const string Title = "Change null checking";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(GenericTypeParameterEmptinessChecking.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);
            var binary = (BinaryExpressionSyntax)syntaxNode.Parent;
            var otherNode = binary.Left == syntaxNode
                ? binary.Right
                : binary.Left;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var typeSymbol = (ITypeParameterSymbol)semanticModel.GetTypeInfo(otherNode).Type;
            var defaultExpression = SyntaxFactory.DefaultExpression(SyntaxFactory.ParseTypeName(typeSymbol.Name));

            ExpressionSyntax newNode = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
                    SyntaxFactory.IdentifierName("Equals")),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(otherNode),
                        SyntaxFactory.Argument(defaultExpression)
                    })));

            if (binary.IsKind(SyntaxKind.NotEqualsExpression))
            {
                newNode = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, newNode);
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = root.ReplaceNode(binary, newNode.WithTriviaFrom(binary));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }
    }
}
