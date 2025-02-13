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

using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CollectionEmptinessCheckingCodeFix : SonarCodeFix
    {
        private const string Title = "Use Any() instead";

        private readonly CSharpFacade language = CSharpFacade.Instance;

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CollectionEmptinessChecking.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            if (root.FindNode(context.Diagnostics.First().Location.SourceSpan)?.FirstAncestorOrSelf<BinaryExpressionSyntax>() is { } binary)
            {
                var binaryLeft = binary.Left;
                var binaryRight = binary.Right;

                if (language.ExpressionNumericConverter.ConstantIntValue(binaryLeft) is { } left)
                {
                    Simplify(root, binary, binaryRight, language.Syntax.ComparisonKind(binary).Mirror().Compare(left), context);
                }
                else if (language.ExpressionNumericConverter.ConstantIntValue(binaryRight) is { } right)
                {
                    Simplify(root, binary, binaryLeft, language.Syntax.ComparisonKind(binary).Compare(right), context);
                }
            }
            return Task.CompletedTask;
        }

        private static void Simplify(SyntaxNode root, ExpressionSyntax expression, ExpressionSyntax countExpression, CountComparisonResult comparisonResult, SonarCodeFixContext context) =>
            context.RegisterCodeFix(
                Title,
                _ => Replacement(root, expression, (InvocationExpressionSyntax)countExpression, comparisonResult, context),
                context.Diagnostics);

        private static Task<Document> Replacement(SyntaxNode root, ExpressionSyntax expression, InvocationExpressionSyntax count, CountComparisonResult comparison, SonarCodeFixContext context)
        {
            var any = IsExtension(count)
                ? AnyFromExtension(count)
                : AnyFromStaticMethod(count);

            SyntaxNode replacement = comparison == CountComparisonResult.Empty
                ? SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, any)
                : any;

            return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(expression, replacement).WithAdditionalAnnotations(Formatter.Annotation)));
        }

        private static InvocationExpressionSyntax AnyFromExtension(InvocationExpressionSyntax count)
        {
            var memberAccess = (MemberAccessExpressionSyntax)count.Expression;
            var name = memberAccess.WithName(SyntaxFactory.IdentifierName(nameof(Enumerable.Any)));
            return SyntaxFactory.InvocationExpression(name, count.ArgumentList);
        }

        private static InvocationExpressionSyntax AnyFromStaticMethod(InvocationExpressionSyntax count)
        {
            var expression = count.ArgumentList.Arguments[0].Expression;
            var name = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, SyntaxFactory.IdentifierName(nameof(Enumerable.Any)));
            var arguments = SyntaxFactory.ArgumentList(count.ArgumentList.Arguments.RemoveAt(0));
            return SyntaxFactory.InvocationExpression(name, arguments);
        }

        private static bool IsExtension(InvocationExpressionSyntax count) =>
            !count.ArgumentList.Arguments.Any()
            || !((MemberAccessExpressionSyntax)count.Expression).Expression.NameIs(nameof(Enumerable));
    }
}
