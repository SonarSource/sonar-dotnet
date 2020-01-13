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
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class ConditionalSimplificationCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Simplify condition";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ConditionalSimplification.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntax = root.FindNode(diagnosticSpan);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            if (syntax is ConditionalExpressionSyntax conditional)
            {
                var condition = conditional.Condition.RemoveParentheses();
                var whenTrue = conditional.WhenTrue.RemoveParentheses();
                var whenFalse = conditional.WhenFalse.RemoveParentheses();
                ConditionalSimplification.TryGetExpressionComparedToNull(condition, out var compared, out var comparedIsNullInTrue);

                var annotation = new SyntaxAnnotation();
                var coalescing = GetNullCoalescing(whenTrue, whenFalse, compared, semanticModel, annotation);

                context.RegisterCodeFix(
                    GetActionToExecute(context, root, conditional, coalescing, annotation),
                    context.Diagnostics);
            }

            if (syntax is IfStatementSyntax ifStatement)
            {
                var whenTrue = ConditionalSimplification.ExtractSingleStatement(ifStatement.Statement);
                var whenFalse = ConditionalSimplification.ExtractSingleStatement(ifStatement.Else.Statement);
                ConditionalSimplification.TryGetExpressionComparedToNull(ifStatement.Condition, out var compared, out var comparedIsNullInTrue);
                var isNullCoalescing = bool.Parse(diagnostic.Properties[ConditionalSimplification.IsNullCoalescingKey]);

                var annotation = new SyntaxAnnotation();
                var simplified = GetSimplified(whenTrue, whenFalse, ifStatement.Condition, compared, semanticModel, annotation, isNullCoalescing);

                context.RegisterCodeFix(
                    GetActionToExecute(context, root, ifStatement, simplified, annotation),
                    context.Diagnostics);
            }
        }

        private static CodeAction GetActionToExecute(CodeFixContext context, SyntaxNode root,
            SyntaxNode nodeToChange, SyntaxNode nodeToAdd, SyntaxAnnotation annotation)
        {
            return CodeAction.Create(
                Title,
                c =>
                {
                    var nodeToAddWithoutAnnotation = RemoveAnnotation(nodeToAdd, annotation);

                    var newRoot = root.ReplaceNode(
                        nodeToChange,
                        nodeToAddWithoutAnnotation.WithTriviaFrom(nodeToChange).WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                });
        }

        private static T RemoveAnnotation<T>(T node, SyntaxAnnotation annotation) where T: SyntaxNode
        {
            var annotated = node.GetAnnotatedNodes(annotation).FirstOrDefault();
            if (annotated == null)
            {
                return node;
            }

            if (annotated == node)
            {
                return node.WithoutAnnotations(annotation);
            }

            return node.ReplaceNode(annotated, annotated.WithoutAnnotations(annotation));
        }

        private static StatementSyntax GetSimplified(StatementSyntax statement1, StatementSyntax statement2,
            ExpressionSyntax condition, ExpressionSyntax compared, SemanticModel semanticModel, SyntaxAnnotation annotation,
            bool isNullCoalescing)
        {
            if (statement1 is ReturnStatementSyntax return1 &&
                statement2 is ReturnStatementSyntax return2)
            {
                var retExpr1 = return1.Expression.RemoveParentheses();
                var retExpr2 = return2.Expression.RemoveParentheses();

                var createdExpression = isNullCoalescing
                    ? GetNullCoalescing(retExpr1, retExpr2, compared, semanticModel, annotation)
                    : GetConditionalExpression(condition, return1.Expression, return2.Expression, annotation);

                return SyntaxFactory.ReturnStatement(createdExpression);
            }

            var expressionStatement1 = statement1 as ExpressionStatementSyntax;
            var expressionStatement2 = statement2 as ExpressionStatementSyntax;

            var expression1 = expressionStatement1.Expression.RemoveParentheses();
            var expression2 = expressionStatement2.Expression.RemoveParentheses();

            var assignment = GetSimplifiedAssignment(expression1, expression2, condition, compared, semanticModel, annotation, isNullCoalescing);
            if (assignment != null)
            {
                return SyntaxFactory.ExpressionStatement(assignment);
            }

            var expression = GetSimplificationFromInvocations(expression1, expression2, condition, compared, semanticModel, annotation, isNullCoalescing);
            return expression != null
                ? SyntaxFactory.ExpressionStatement(expression)
                : null;
        }

        private static ConditionalExpressionSyntax GetConditionalExpression(ExpressionSyntax condition, ExpressionSyntax expressionTrue,
            ExpressionSyntax expressionFalse, SyntaxAnnotation annotation)
        {
            return SyntaxFactory.ConditionalExpression(
                condition,
                expressionTrue,
                expressionFalse)
                .WithAdditionalAnnotations(annotation);
        }

        private static ExpressionSyntax GetSimplifiedAssignment(ExpressionSyntax expression1, ExpressionSyntax expression2,
            ExpressionSyntax condition, ExpressionSyntax compared, SemanticModel semanticModel, SyntaxAnnotation annotation,
            bool isNullCoalescing)
        {
            var assignment1 = expression1 as AssignmentExpressionSyntax;
            var assignment2 = expression2 as AssignmentExpressionSyntax;
            var canBeSimplified =
                assignment1 != null &&
                assignment2 != null &&
                CSharpEquivalenceChecker.AreEquivalent(assignment1.Left, assignment2.Left) &&
                assignment1.Kind() == assignment2.Kind();

            if (!canBeSimplified)
            {
                return null;
            }

            var createdExpression = isNullCoalescing
                ? GetNullCoalescing(assignment1.Right, assignment2.Right, compared, semanticModel, annotation)
                : GetConditionalExpression(condition, assignment1.Right, assignment2.Right, annotation);

            return SyntaxFactory.AssignmentExpression(
                assignment1.Kind(),
                assignment1.Left,
                createdExpression);
        }

        private static ExpressionSyntax GetNullCoalescing(ExpressionSyntax whenTrue, ExpressionSyntax whenFalse,
            ExpressionSyntax compared, SemanticModel semanticModel, SyntaxAnnotation annotation)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(whenTrue, compared))
            {
                var createdExpression = SyntaxFactory.BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    compared,
                    whenFalse)
                    .WithAdditionalAnnotations(annotation);
                return createdExpression;
            }

            if (CSharpEquivalenceChecker.AreEquivalent(whenFalse, compared))
            {
                var createdExpression = SyntaxFactory.BinaryExpression(
                    SyntaxKind.CoalesceExpression,
                    compared,
                    whenTrue)
                    .WithAdditionalAnnotations(annotation);
                return createdExpression;
            }

            return GetSimplificationFromInvocations(whenTrue, whenFalse, null, compared, semanticModel, annotation,
                isNullCoalescing: true);
        }

        private static ExpressionSyntax GetSimplificationFromInvocations(ExpressionSyntax expression1, ExpressionSyntax expression2,
            ExpressionSyntax condition, ExpressionSyntax compared, SemanticModel semanticModel, SyntaxAnnotation annotation,
            bool isNullCoalescing)
        {
            var methodCall2 = expression2 as InvocationExpressionSyntax;
            if (!(expression1 is InvocationExpressionSyntax methodCall1) ||
                methodCall2 == null)
            {
                return null;
            }

            var methodSymbol1 = semanticModel.GetSymbolInfo(methodCall1).Symbol;
            var methodSymbol2 = semanticModel.GetSymbolInfo(methodCall2).Symbol;
            if (methodSymbol1 == null ||
                methodSymbol2 == null ||
                !methodSymbol1.Equals(methodSymbol2))
            {
                return null;
            }

            var newArgumentList = SyntaxFactory.ArgumentList();

            for (var i = 0; i < methodCall1.ArgumentList.Arguments.Count; i++)
            {
                var arg1 = methodCall1.ArgumentList.Arguments[i];
                var arg2 = methodCall2.ArgumentList.Arguments[i];

                var expr1 = arg1.Expression.RemoveParentheses();
                var expr2 = arg2.Expression.RemoveParentheses();

                if (!CSharpEquivalenceChecker.AreEquivalent(expr1, expr2))
                {
                    ExpressionSyntax createdExpression;
                    if (isNullCoalescing)
                    {
                        var arg1IsCompared = CSharpEquivalenceChecker.AreEquivalent(expr1, compared);
                        var expression = arg1IsCompared ? expr2 : expr1;

                        createdExpression = SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, compared, expression);
                    }
                    else
                    {
                        createdExpression = SyntaxFactory.ConditionalExpression(condition, expr1, expr2);
                    }

                    newArgumentList = newArgumentList.AddArguments(
                        SyntaxFactory.Argument(
                            arg1.NameColon,
                            arg1.RefOrOutKeyword,
                            createdExpression.WithAdditionalAnnotations(annotation)));
                }
                else
                {
                    newArgumentList = newArgumentList.AddArguments(arg1.WithExpression(arg1.Expression.RemoveParentheses()));
                }
            }

            return methodCall1.WithArgumentList(newArgumentList);
        }
    }
}

