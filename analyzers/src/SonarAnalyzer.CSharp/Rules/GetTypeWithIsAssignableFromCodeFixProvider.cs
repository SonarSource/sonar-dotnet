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
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
            var invocation = syntaxNode as InvocationExpressionSyntax;
            var binary = syntaxNode as BinaryExpressionSyntax;
            if (invocation == null && binary == null)
            {
                return TaskHelper.CompletedTask;
            }

            if (TryGetNewRoot(root, diagnostic, invocation, binary, out var newRoot))
            {
                context.RegisterCodeFix(CodeAction.Create(Title, c => Task.FromResult(context.Document.WithSyntaxRoot(newRoot))), context.Diagnostics);
            }

            return TaskHelper.CompletedTask;
        }

        private static bool TryGetNewRoot(SyntaxNode root, Diagnostic diagnostic, InvocationExpressionSyntax invocation, BinaryExpressionSyntax binary, out SyntaxNode newRoot)
        {
            if (invocation != null)
            {
                newRoot = ChangeInvocation(root, diagnostic, invocation);
            }
            else if (binary.IsKind(SyntaxKind.IsExpression))
            {
                newRoot = ChangeIsExpressionToNullCheck(root, binary);
            }
            else if (RefactoredExpression(binary) is { } expression)
            {
                newRoot = root.ReplaceNode(binary, expression.WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                newRoot = null;
            }

            return newRoot != null;
        }

        private static SyntaxNode ChangeIsExpressionToNullCheck(SyntaxNode root, BinaryExpressionSyntax binary)
        {
            var newNode = ExpressionWithParensIfNeeded(GetNullCheck(binary), binary.Parent);
            return root.ReplaceNode(binary, newNode.WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static ExpressionSyntax GetNullCheck(BinaryExpressionSyntax binary) =>
            SyntaxFactory.BinaryExpression(SyntaxKind.NotEqualsExpression, binary.Left.RemoveParentheses(), CSharpSyntaxHelper.NullLiteralExpression);

        private static SyntaxNode ChangeInvocation(SyntaxNode root, Diagnostic diagnostic, InvocationExpressionSyntax invocation)
        {
            var useIsOperator = bool.Parse(diagnostic.Properties[GetTypeWithIsAssignableFrom.UseIsOperatorKey]);
            var shouldRemoveGetType = bool.Parse(diagnostic.Properties[GetTypeWithIsAssignableFrom.ShouldRemoveGetTypeKey]);
            var newNode = RefactoredExpression(invocation, useIsOperator, shouldRemoveGetType);
            return root.ReplaceNode(invocation, newNode.WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static ExpressionSyntax RefactoredExpression(BinaryExpressionSyntax binary)
        {
            ExpressionSyntax newExpression;
            var noNegationRequired = binary.IsKind(SyntaxKind.EqualsExpression);

            if (TryGetTypeOfComparison(binary, out var typeofExpression, out var getTypeSide))
            {
                newExpression = CreateIsExpression(typeofExpression, getTypeSide, shouldRemoveGetType: true);
            }
            else if (AsOperatorComparisonToNull(binary) is { } asExpression)
            {
                newExpression = GetIsExpression(asExpression);
                noNegationRequired = !noNegationRequired;
            }
            else
            {
                return null;
            }

            return noNegationRequired
                ? ExpressionWithParensIfNeeded(newExpression, binary.Parent)
                : SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(newExpression));
        }

        private static ExpressionSyntax RefactoredExpression(InvocationExpressionSyntax invocation, bool useIsOperator, bool shouldRemoveGetType)
        {
            var typeInstance = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;
            var getTypeCallInArgument = invocation.ArgumentList.Arguments.First();
            return useIsOperator
                ? ExpressionWithParensIfNeeded(CreateIsExpression(typeInstance, getTypeCallInArgument.Expression, shouldRemoveGetType), invocation.Parent)
                : IsInstanceOfTypeCall(invocation, typeInstance, getTypeCallInArgument);
        }

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
