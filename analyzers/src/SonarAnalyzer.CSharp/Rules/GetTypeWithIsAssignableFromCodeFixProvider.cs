/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class GetTypeWithIsAssignableFromCodeFixProvider : SonarCodeFixProvider
    {
        private const string Title = "Simplify type checking";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(GetTypeWithIsAssignableFrom.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
            if (NewRoot(root, diagnostic, node) is { } newRoot)
            {
                context.RegisterCodeFix(CodeAction.Create(Title, c => Task.FromResult(context.Document.WithSyntaxRoot(newRoot))), context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        private static SyntaxNode NewRoot(SyntaxNode root, Diagnostic diagnostic, SyntaxNode node) =>
            node switch {
                InvocationExpressionSyntax invocation => ChangeInvocation(root, diagnostic, invocation),
                BinaryExpressionSyntax binary => ChangeBinary(root, binary),
                var _ when node.IsKind(SyntaxKindEx.IsPatternExpression) => ChangeIsPattern(root, (IsPatternExpressionSyntaxWrapper)node),
                _ => null
            };

        private static SyntaxNode ChangeInvocation(SyntaxNode root, Diagnostic diagnostic, InvocationExpressionSyntax invocation)
        {
            var useIsOperator = bool.Parse(diagnostic.Properties[GetTypeWithIsAssignableFrom.UseIsOperatorKey]);
            var shouldRemoveGetType = bool.Parse(diagnostic.Properties[GetTypeWithIsAssignableFrom.ShouldRemoveGetTypeKey]);
            var newNode = RefactoredExpression(invocation, useIsOperator, shouldRemoveGetType);
            return root.ReplaceNode(invocation, newNode.WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static SyntaxNode ChangeBinary(SyntaxNode root, BinaryExpressionSyntax binary)
        {
            if (binary.IsKind(SyntaxKind.IsExpression))
            {
                return ChangeIsExpressionToNullCheck(root, binary);
            }
            else if (RefactoredExpression(binary) is { } expression)
            {
                return root.ReplaceNode(binary, expression.WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                return null;
            }
        }

        private static SyntaxNode ChangeIsPattern(SyntaxNode root, IsPatternExpressionSyntaxWrapper isPattern)
        {
            if (isPattern.Expression is BinaryExpressionSyntax binary)
            {
                var negationRequired = true;
                var current = isPattern.Pattern;
                while (current.SyntaxNode.IsKind(SyntaxKindEx.NotPattern))
                {
                    negationRequired = !negationRequired;
                    current = ((UnaryPatternSyntaxWrapper)current).Pattern;
                }
                var newExpression = NegatedExpression(negationRequired, isPattern.SyntaxNode.Parent, GetIsExpression(binary));
                return root.ReplaceNode(isPattern, newExpression.WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                return null;
            }
        }

        private static SyntaxNode ChangeIsExpressionToNullCheck(SyntaxNode root, BinaryExpressionSyntax binary)
        {
            var newNullCheck = NullCheck(binary);
            var newExpression = RefactoredExpression(newNullCheck) ?? newNullCheck; // Try to improve nested cases
            return root.ReplaceNode(binary, ExpressionWithParensIfNeeded(newExpression, binary.Parent).WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static BinaryExpressionSyntax NullCheck(BinaryExpressionSyntax binary) =>
            SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, binary.Left.RemoveParentheses(), CSharpSyntaxHelper.NullLiteralExpression);

        private static ExpressionSyntax RefactoredExpression(BinaryExpressionSyntax binary)
        {
            ExpressionSyntax newExpression;
            var negationRequired = binary.IsKind(SyntaxKind.NotEqualsExpression);

            if (TryGetTypeOfComparison(binary, out var typeofExpression, out var getTypeSide))
            {
                newExpression = CreateIsExpression(typeofExpression, getTypeSide, shouldRemoveGetType: true);
            }
            else if (AsOperatorComparisonToNull(binary) is { } asExpression)
            {
                newExpression = GetIsExpression(asExpression);
                negationRequired = !negationRequired;
            }
            else
            {
                return null;
            }

            return NegatedExpression(negationRequired, binary.Parent, newExpression);
        }

        private static ExpressionSyntax RefactoredExpression(InvocationExpressionSyntax invocation, bool useIsOperator, bool shouldRemoveGetType)
        {
            var typeInstance = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;
            var getTypeCallInArgument = invocation.ArgumentList.Arguments.First();
            return useIsOperator
                ? ExpressionWithParensIfNeeded(CreateIsExpression(typeInstance, getTypeCallInArgument.Expression, shouldRemoveGetType), invocation.Parent)
                : IsInstanceOfTypeCall(invocation, typeInstance, getTypeCallInArgument);
        }

        private static ExpressionSyntax NegatedExpression(bool negationRequired, SyntaxNode parent, ExpressionSyntax expression) =>
            negationRequired
                ? SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(expression))
                : ExpressionWithParensIfNeeded(expression, parent);

        private static ExpressionSyntax GetIsExpression(BinaryExpressionSyntax asExpression) =>
            SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, asExpression.Left, asExpression.Right).WithAdditionalAnnotations(Formatter.Annotation);

        private static BinaryExpressionSyntax AsOperatorComparisonToNull(BinaryExpressionSyntax binary)
        {
            var left = binary.Left.RemoveParentheses();
            return left.IsKind(SyntaxKind.AsExpression)
                ? left as BinaryExpressionSyntax
                : binary.Right.RemoveParentheses() as BinaryExpressionSyntax;
        }

        private static bool TryGetTypeOfComparison(BinaryExpressionSyntax binary, out TypeOfExpressionSyntax typeofExpression, out ExpressionSyntax getTypeSide)
        {
            typeofExpression = binary.Left as TypeOfExpressionSyntax;
            getTypeSide = binary.Right;
            if (typeofExpression == null)
            {
                typeofExpression = binary.Right as TypeOfExpressionSyntax;
                getTypeSide = binary.Left;
            }
            return typeofExpression != null;
        }

        private static InvocationExpressionSyntax IsInstanceOfTypeCall(InvocationExpressionSyntax invocation, ExpressionSyntax typeInstance, ArgumentSyntax getTypeCallInArgument) =>
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, typeInstance, SyntaxFactory.IdentifierName("IsInstanceOfType")).WithTriviaFrom(invocation.Expression),
                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(ExpressionFromGetType(getTypeCallInArgument.Expression)).WithTriviaFrom(getTypeCallInArgument) }))
                                           .WithTriviaFrom(invocation.ArgumentList))
                .WithTriviaFrom(invocation);

        private static ExpressionSyntax ExpressionWithParensIfNeeded(ExpressionSyntax expression, SyntaxNode parent) =>
            (parent is ExpressionSyntax && !(parent is AssignmentExpressionSyntax) && !(parent is ParenthesizedExpressionSyntax))
                ? SyntaxFactory.ParenthesizedExpression(expression)
                : expression;

        private static ExpressionSyntax CreateIsExpression(ExpressionSyntax typeInstance, ExpressionSyntax getTypeCall, bool shouldRemoveGetType) =>
            SyntaxFactory.BinaryExpression(SyntaxKind.IsExpression, shouldRemoveGetType ? ExpressionFromGetType(getTypeCall) : getTypeCall, ((TypeOfExpressionSyntax)typeInstance).Type);

        private static ExpressionSyntax ExpressionFromGetType(ExpressionSyntax getTypeCall) =>
            ((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)getTypeCall).Expression).Expression;
    }
}
