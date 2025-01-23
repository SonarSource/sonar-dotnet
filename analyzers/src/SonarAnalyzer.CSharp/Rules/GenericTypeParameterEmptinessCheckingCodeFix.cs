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

            var semanticModel = await context.Document.GetSemanticModelAsync(context.Cancel).ConfigureAwait(false);
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
