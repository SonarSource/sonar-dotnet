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

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ConditionalSimplification.DiagnosticId);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var oldNode = root.FindNode(diagnostic.Location.SourceSpan);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var newNode = Simplify(diagnostic, semanticModel, oldNode, out var annotation);
            if (newNode != null)
            {
                context.RegisterCodeFix(CodeAction.Create(Title, c =>
                    {
                        var nodeToAddWithoutAnnotation = RemoveAnnotation(newNode, annotation);

                        var newRoot = root.ReplaceNode(
                            oldNode,
                            nodeToAddWithoutAnnotation.WithTriviaFrom(oldNode).WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }), context.Diagnostics);
            }
        }

        private static SyntaxNode Simplify(Diagnostic diagnostic, SemanticModel semanticModel, SyntaxNode oldNode, out SyntaxAnnotation annotation)
        {
            ExpressionSyntax compared;
            switch (oldNode)
            {
                case ConditionalExpressionSyntax conditional:
                    var condition = conditional.Condition.RemoveParentheses();
                    var whenTrue = conditional.WhenTrue.RemoveParentheses();
                    var whenFalse = conditional.WhenFalse.RemoveParentheses();
                    ConditionalSimplification.TryGetExpressionComparedToNull(condition, out compared, out _);
                    return SimplifyCoalesceExpression(new ComparedContext(diagnostic, semanticModel, compared, out annotation), whenTrue, whenFalse);

                case IfStatementSyntax ifStatement:
                    var ifPart = ConditionalSimplification.ExtractSingleStatement(ifStatement.Statement);
                    var elsePart = ConditionalSimplification.ExtractSingleStatement(ifStatement.Else?.Statement);
                    ConditionalSimplification.TryGetExpressionComparedToNull(ifStatement.Condition, out compared, out var _);
                    return SimplifyIfStatement(new ComparedContext(diagnostic, semanticModel, compared, out annotation), ifPart, elsePart, ifStatement.Condition.RemoveParentheses());

                case AssignmentExpressionSyntax assignment:
                    var context = new ComparedContext(diagnostic, semanticModel, null, out annotation);
                    var right = assignment.Right.RemoveParentheses();
                    if (right is BinaryExpressionSyntax binaryExpression && binaryExpression.Kind() == SyntaxKind.CoalesceExpression)
                    {
                        return CoalesceAssignmentExpression(context, assignment.Left, binaryExpression.Right);
                    }
                    else if (right is ConditionalExpressionSyntax conditional)
                    {
                        ConditionalSimplification.TryGetExpressionComparedToNull(conditional.Condition, out compared, out var comparedIsNullInTrue);
                        if (context.IsCoalesceAssignmentSupported && ConditionalSimplification.IsCoalesceAssignmentCandidate(conditional, compared))
                        {
                            return CoalesceAssignmentExpression(context,
                                (conditional.GetFirstNonParenthesizedParent() as AssignmentExpressionSyntax).Left,
                                (comparedIsNullInTrue ? conditional.WhenTrue : conditional.WhenFalse).RemoveParentheses());
                        }
                    }
                    break;
            }
            annotation = null;
            return null;
        }

        private static T RemoveAnnotation<T>(T node, SyntaxAnnotation annotation) where T : SyntaxNode
        {
            var annotated = node.GetAnnotatedNodes(annotation).FirstOrDefault();
            if (annotated == null)
            {
                return node;
            }
            else if (annotated == node)
            {
                return node.WithoutAnnotations(annotation);
            }
            return node.ReplaceNode(annotated, annotated.WithoutAnnotations(annotation));
        }

        private static StatementSyntax SimplifyIfStatement(ComparedContext context, StatementSyntax statement1, StatementSyntax statement2, ExpressionSyntax condition)
        {
            if (statement1 is ReturnStatementSyntax return1 && statement2 is ReturnStatementSyntax return2)
            {
                var retExpr1 = return1.Expression.RemoveParentheses();
                var retExpr2 = return2.Expression.RemoveParentheses();
                var createdExpression = context.SimplifiedOperator switch
                {
                    "?:" => ConditionalExpression(condition, retExpr1, retExpr2, context.Annotation),
                    "??" => SimplifyCoalesceExpression(context, retExpr1, retExpr2),
                    _ => throw new System.InvalidOperationException("Unexpected simplifiedOperator: " + context.SimplifiedOperator)
                };
                return SyntaxFactory.ReturnStatement(createdExpression);
            }
            var expression = SimplifyIfExpression(context, statement1 as ExpressionStatementSyntax, statement2 as ExpressionStatementSyntax, condition);
            return expression == null ? null : SyntaxFactory.ExpressionStatement(expression);
        }

        private static ExpressionSyntax SimplifyIfExpression(ComparedContext context, ExpressionStatementSyntax statement1, ExpressionStatementSyntax statement2, ExpressionSyntax condition)
        {
            bool isCoalescing;
            var expression1 = statement1.Expression.RemoveParentheses();
            switch (context.SimplifiedOperator)
            {
                case "?:":
                    isCoalescing = false;
                    break;
                case "??":
                    if (statement2 == null)
                    {
                        return CoalesceAssignmentExpression(context, expression1);
                    }
                    isCoalescing = true;
                    break;
                case "??=":
                    return CoalesceAssignmentExpression(context, expression1);
                default:
                    throw new System.InvalidOperationException("Unexpected simplifiedOperator: " + context.SimplifiedOperator);
            }
            var expression2 = statement2.Expression.RemoveParentheses();
            return SimplifyAssignmentExpression(context, expression1, expression2, condition, isCoalescing)
                ?? SimplifyInvocationExpression(context, expression1, expression2, condition, isCoalescing);
        }

        private static ExpressionSyntax SimplifyAssignmentExpression(ComparedContext context, ExpressionSyntax expression1, ExpressionSyntax expression2, ExpressionSyntax condition, bool isCoalescing)
        {
            if (expression1 is AssignmentExpressionSyntax assignment1
                && expression2 is AssignmentExpressionSyntax assignment2
                && assignment1.Kind() == assignment2.Kind()
                && CSharpEquivalenceChecker.AreEquivalent(assignment1.Left, assignment2.Left))
            {
                var createdExpression = isCoalescing
                    ? SimplifyCoalesceExpression(context, assignment1.Right, assignment2.Right)
                    : ConditionalExpression(condition, assignment1.Right, assignment2.Right, context.Annotation);
                return SyntaxFactory.AssignmentExpression(assignment1.Kind(), assignment1.Left, createdExpression);
            }
            return null;
        }

        private static ExpressionSyntax SimplifyCoalesceExpression(ComparedContext context, ExpressionSyntax whenTrue, ExpressionSyntax whenFalse)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(whenTrue, context.Compared))
            {
                return CoalesceExpression(context.Compared, whenFalse, context.Annotation);
            }
            else if (CSharpEquivalenceChecker.AreEquivalent(whenFalse, context.Compared))
            {
                return CoalesceExpression(context.Compared, whenTrue, context.Annotation);
            }
            return SimplifyInvocationExpression(context, whenTrue, whenFalse, null, isCoalescing: true);
        }

        private static ExpressionSyntax CoalesceAssignmentExpression(ComparedContext context, ExpressionSyntax assignmentExpression) =>
            assignmentExpression is AssignmentExpressionSyntax originalAssignment
                ? CoalesceAssignmentExpression(context, originalAssignment.Left, originalAssignment.Right)
                : null;

        private static ExpressionSyntax CoalesceAssignmentExpression(ComparedContext context, ExpressionSyntax left, ExpressionSyntax right)
        {
            if (context.IsCoalesceAssignmentSupported)
            {
                return SyntaxFactory.AssignmentExpression(ShimLayer.CSharp.SyntaxKindEx.CoalesceAssignmentExpression, left, right).WithAdditionalAnnotations(context.Annotation);
            }
            return SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, CoalesceExpression(left, right, context.Annotation));
        }

        private static ExpressionSyntax CoalesceExpression(ExpressionSyntax left, ExpressionSyntax right, SyntaxAnnotation annotation) =>
            SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, left, right).WithAdditionalAnnotations(annotation);

        private static ConditionalExpressionSyntax ConditionalExpression(ExpressionSyntax condition, ExpressionSyntax truePart, ExpressionSyntax falsePart, SyntaxAnnotation annotation) =>
            SyntaxFactory.ConditionalExpression(condition, truePart, falsePart).WithAdditionalAnnotations(annotation);

        private static ExpressionSyntax SimplifyInvocationExpression(ComparedContext context, ExpressionSyntax expression1, ExpressionSyntax expression2, ExpressionSyntax condition, bool isCoalescing)
        {
            if (expression1 is InvocationExpressionSyntax methodCall1
                && expression2 is InvocationExpressionSyntax methodCall2
                && context.SemanticModel.GetSymbolInfo(methodCall1).Symbol is IMethodSymbol methodSymbol1
                && context.SemanticModel.GetSymbolInfo(methodCall2).Symbol is IMethodSymbol methodSymbol2
                && methodSymbol1.Equals(methodSymbol2))
            {
                return methodCall1.WithArgumentList(SimplifyInvocationArguments(context, methodCall1, methodCall2, condition, isCoalescing));
            }
            return null;
        }

        private static ArgumentListSyntax SimplifyInvocationArguments(ComparedContext context, InvocationExpressionSyntax methodCall1, InvocationExpressionSyntax methodCall2, ExpressionSyntax condition, bool isCoalescing)
        {
            var newArgumentList = SyntaxFactory.ArgumentList();
            for (var i = 0; i < methodCall1.ArgumentList.Arguments.Count; i++)
            {
                var arg1 = methodCall1.ArgumentList.Arguments[i];
                var arg2 = methodCall2.ArgumentList.Arguments[i];
                var expr1 = arg1.Expression.RemoveParentheses();
                var expr2 = arg2.Expression.RemoveParentheses();
                if (CSharpEquivalenceChecker.AreEquivalent(expr1, expr2))
                {
                    newArgumentList = newArgumentList.AddArguments(arg1.WithExpression(expr1));
                }
                else
                {
                    ExpressionSyntax createdExpression;
                    if (isCoalescing)
                    {
                        var arg1Compared = CSharpEquivalenceChecker.AreEquivalent(expr1, context.Compared);
                        createdExpression = SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, context.Compared, arg1Compared ? expr2 : expr1);
                    }
                    else
                    {
                        createdExpression = SyntaxFactory.ConditionalExpression(condition, expr1, expr2);
                    }
                    newArgumentList = newArgumentList.AddArguments(SyntaxFactory.Argument(arg1.NameColon, arg1.RefOrOutKeyword, createdExpression.WithAdditionalAnnotations(context.Annotation)));
                }
            }
            return newArgumentList;
        }

        private class ComparedContext
        {
            public readonly ExpressionSyntax Compared;
            public readonly SemanticModel SemanticModel;
            public readonly SyntaxAnnotation Annotation;
            public readonly bool IsCoalesceAssignmentSupported;
            /// <summary>
            /// Value is set only for IfStatement checks.
            /// </summary>
            public readonly string SimplifiedOperator;

            public ComparedContext(Diagnostic diagnostic, SemanticModel semanticModel, ExpressionSyntax compared, out SyntaxAnnotation annotation)
            {
                this.Compared = compared;
                this.SemanticModel = semanticModel;
                this.Annotation = new SyntaxAnnotation();
                this.IsCoalesceAssignmentSupported = bool.Parse(diagnostic.Properties[ConditionalSimplification.IsCoalesceAssignmentSupportedKey]);
                diagnostic.Properties.TryGetValue(ConditionalSimplification.SimplifiedOperatorKey, out this.SimplifiedOperator);
                annotation = this.Annotation;
            }
        }
    }
}

