/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class GetTypeWithIsAssignableFromCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Simplify type checking";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(GetTypeWithIsAssignableFrom.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
            var invocation = syntaxNode as InvocationExpressionSyntax;
            var binary = syntaxNode as BinaryExpressionSyntax;
            if (invocation == null && binary == null)
            {
                return TaskHelper.CompletedTask;
            }

            if (!TryGetNewRoot(root, diagnostic, invocation, binary, out var newRoot))
            {
                return TaskHelper.CompletedTask;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => Task.FromResult(context.Document.WithSyntaxRoot(newRoot))),
                context.Diagnostics);

            return TaskHelper.CompletedTask;
        }

        private static bool TryGetNewRoot(SyntaxNode root, Diagnostic diagnostic, InvocationExpressionSyntax invocation,
            BinaryExpressionSyntax binary, out SyntaxNode newRoot)
        {
            newRoot = null;
            if (invocation != null)
            {
                newRoot = ChangeInvocation(root, diagnostic, invocation);
            }
            else if (binary.IsKind(SyntaxKind.IsExpression))
            {
                newRoot = ChangeIsExpressionToNullCheck(root, binary);
            }
            else
            {
                if (TryGetRefactoredExpression(binary, out var expression))
                {
                    newRoot = root.ReplaceNode(binary, expression.WithAdditionalAnnotations(Formatter.Annotation));
                }
            }

            return newRoot != null;
        }

        private static SyntaxNode ChangeIsExpressionToNullCheck(SyntaxNode root, BinaryExpressionSyntax binary)
        {
            var newNode = GetExpressionWithParensIfNeeded(GetNullCheck(binary), binary.Parent);
            return root.ReplaceNode(binary, newNode.WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static ExpressionSyntax GetNullCheck(BinaryExpressionSyntax binary)
        {
            return SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression,
                binary.Left.RemoveParentheses(),
                CSharpSyntaxHelper.NullLiteralExpression);
        }

        private static SyntaxNode ChangeInvocation(SyntaxNode root, Diagnostic diagnostic,
            InvocationExpressionSyntax invocation)
        {
            var useIsOperator = bool.Parse(diagnostic.Properties[GetTypeWithIsAssignableFrom.UseIsOperatorKey]);
            var shouldRemoveGetType = bool.Parse(diagnostic.Properties[GetTypeWithIsAssignableFrom.ShouldRemoveGetType]);

            var newNode = GetRefactoredExpression(invocation, useIsOperator, shouldRemoveGetType);
            return root.ReplaceNode(invocation, newNode.WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static bool TryGetRefactoredExpression(BinaryExpressionSyntax binary, out ExpressionSyntax expression)
        {

            var noNegationRequired = binary.IsKind(SyntaxKind.EqualsExpression);
            ExpressionSyntax newExpression;

            if (TryGetTypeOfComparison(binary, out var typeofExpression, out var getTypeSide))
            {
                newExpression = GetIsExpression(typeofExpression, getTypeSide, shouldRemoveGetType: true);
            }
            else if (TryGetAsOperatorComparisonToNull(binary, out var asExpression))
            {
                newExpression = GetIsExpression(asExpression);
                noNegationRequired = !noNegationRequired;
            }
            else
            {
                expression = null;
                return false;
            }

            expression = noNegationRequired
                ? GetExpressionWithParensIfNeeded(newExpression, binary.Parent)
                : SyntaxFactory.PrefixUnaryExpression(
                    SyntaxKind.LogicalNotExpression,
                    SyntaxFactory.ParenthesizedExpression(newExpression));
            return true;
        }

        private static ExpressionSyntax GetIsExpression(BinaryExpressionSyntax asExpression)
        {
            return SyntaxFactory.BinaryExpression(
                SyntaxKind.IsExpression,
                asExpression.Left,
                asExpression.Right)
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static bool TryGetAsOperatorComparisonToNull(BinaryExpressionSyntax binary,
            out BinaryExpressionSyntax asExpression)
        {
            var left = binary.Left.RemoveParentheses();

            asExpression = left.IsKind(SyntaxKind.AsExpression)
                ? left as BinaryExpressionSyntax
                : binary.Right.RemoveParentheses() as BinaryExpressionSyntax;

            return asExpression != null;
        }

        private static bool TryGetTypeOfComparison(BinaryExpressionSyntax binary,
            out TypeOfExpressionSyntax typeofExpression, out ExpressionSyntax getTypeSide)
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

        private static ExpressionSyntax GetRefactoredExpression(InvocationExpressionSyntax invocation,
            bool useIsOperator, bool shouldRemoveGetType)
        {
            var typeInstance = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;
            var getTypeCallInArgument = invocation.ArgumentList.Arguments.First();

            return useIsOperator
                ? GetExpressionWithParensIfNeeded(
                    GetIsExpression(typeInstance, getTypeCallInArgument.Expression, shouldRemoveGetType),
                    invocation.Parent)
                : GetIsInstanceOfTypeCall(invocation, typeInstance, getTypeCallInArgument);
        }

        private static InvocationExpressionSyntax GetIsInstanceOfTypeCall(InvocationExpressionSyntax invocation,
            ExpressionSyntax typeInstance, ArgumentSyntax getTypeCallInArgument)
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeInstance,
                    SyntaxFactory.IdentifierName("IsInstanceOfType")).WithTriviaFrom(invocation.Expression),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[]
                    {
                        SyntaxFactory.Argument(
                            GetExpressionFromGetType(getTypeCallInArgument.Expression)).WithTriviaFrom(getTypeCallInArgument)
                    }))
                    .WithTriviaFrom(invocation.ArgumentList))
                .WithTriviaFrom(invocation);
        }

        private static ExpressionSyntax GetExpressionWithParensIfNeeded(ExpressionSyntax expression, SyntaxNode parent)
        {
            return (parent is ExpressionSyntax && !(parent is AssignmentExpressionSyntax) && !(parent is ParenthesizedExpressionSyntax))
                ? SyntaxFactory.ParenthesizedExpression(expression)
                : expression;
        }

        private static ExpressionSyntax GetIsExpression(ExpressionSyntax typeInstance,
            ExpressionSyntax getTypeCall, bool shouldRemoveGetType)
        {
            var expression = shouldRemoveGetType
                    ? GetExpressionFromGetType(getTypeCall)
                    : getTypeCall;

            return SyntaxFactory.BinaryExpression(
                SyntaxKind.IsExpression,
                expression,
                ((TypeOfExpressionSyntax)typeInstance).Type);
        }

        private static ExpressionSyntax GetExpressionFromGetType(ExpressionSyntax getTypeCall)
        {
            return ((MemberAccessExpressionSyntax)((InvocationExpressionSyntax)getTypeCall).Expression).Expression;
        }
    }
}
