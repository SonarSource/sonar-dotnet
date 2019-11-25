/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ConditionalSimplification : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3240";
        private const string MessageFormat = "Use the '{0}' operator here.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        internal const string IsNullCoalescingKey = "isNullCoalescing";

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckConditionalExpression,
                SyntaxKind.ConditionalExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckIfStatement,
                SyntaxKind.IfStatement);
        }

        private static void CheckIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            if (ifStatement.Else == null ||
                ifStatement.Parent is ElseClauseSyntax)
            {
                return;
            }

            var whenTrue = ExtractSingleStatement(ifStatement.Statement);
            var whenFalse = ExtractSingleStatement(ifStatement.Else.Statement);

            if (whenTrue == null ||
                whenFalse == null ||
                CSharpEquivalenceChecker.AreEquivalent(whenTrue, whenFalse))
            {
                /// Equivalence handled by S1871, <see cref="ConditionalStructureSameImplementation"/>
                return;
            }
            var possiblyNullCoalescing =
                TryGetExpressionComparedToNull(ifStatement.Condition, out var comparedToNull, out var comparedIsNullInTrue) &&
                ExpressionCanBeNull(comparedToNull, context.SemanticModel);

            if (CanBeSimplified(whenTrue, whenFalse,
                possiblyNullCoalescing ? comparedToNull : null, context.SemanticModel,
                comparedIsNullInTrue, out var isNullCoalescing))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, ifStatement.IfKeyword.GetLocation(),
                    ImmutableDictionary<string, string>.Empty.Add(IsNullCoalescingKey, isNullCoalescing.ToString()),
                    isNullCoalescing ? "??" : "?:"));
            }
        }

        private static void CheckConditionalExpression(SyntaxNodeAnalysisContext context)
        {
            var conditional = (ConditionalExpressionSyntax)context.Node;

            var condition = conditional.Condition.RemoveParentheses();
            var whenTrue = conditional.WhenTrue.RemoveParentheses();
            var whenFalse = conditional.WhenFalse.RemoveParentheses();

            if (CSharpEquivalenceChecker.AreEquivalent(whenTrue, whenFalse))
            {
                /// handled by S2758, <see cref="TernaryOperatorPointless"/>
                return;
            }
            if (!TryGetExpressionComparedToNull(condition, out var comparedToNull, out var comparedIsNullInTrue) ||
                !ExpressionCanBeNull(comparedToNull, context.SemanticModel))
            {
                // expression not compared to null, or can't be null
                return;
            }

            if (CanExpressionBeNullCoalescing(whenTrue, whenFalse, comparedToNull, context.SemanticModel, comparedIsNullInTrue))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, conditional.GetLocation(), "??"));
            }
        }

        private static bool AreTypesCompatible(ExpressionSyntax expression1, ExpressionSyntax expression2, SemanticModel semanticModel)
        {
            if (expression1 is AnonymousFunctionExpressionSyntax || expression2 is AnonymousFunctionExpressionSyntax)
            {
                return false;
            }

            var type1 = semanticModel.GetTypeInfo(expression1).Type;
            var type2 = semanticModel.GetTypeInfo(expression2).Type;

            if (type1 is IErrorTypeSymbol || type2 is IErrorTypeSymbol)
            {
                return false;
            }

            if (CheckNullAndValueType(type1, type2) ||
                CheckNullAndValueType(type2, type1))
            {
                return false;
            }

            if (type2 == null || type1 == null)
            {
                return true;
            }

            return type1.Equals(type2);
        }

        private static bool CheckNullAndValueType(ITypeSymbol typeNull, ITypeSymbol typeValue)
        {
            return typeNull == null && typeValue != null && typeValue.IsValueType;
        }

        private static bool CanBeSimplified(StatementSyntax statement1, StatementSyntax statement2,
            ExpressionSyntax comparedToNull, SemanticModel semanticModel, bool comparedIsNullInTrue, out bool isNullCoalescing)
        {
            isNullCoalescing = false;

            if (statement1 is ReturnStatementSyntax return1 &&
                statement2 is ReturnStatementSyntax return2)
            {
                var retExpr1 = return1.Expression.RemoveParentheses();
                var retExpr2 = return2.Expression.RemoveParentheses();

                if (!AreTypesCompatible(return1.Expression, return2.Expression, semanticModel))
                {
                    return false;
                }

                if (comparedToNull != null &&
                    CanExpressionBeNullCoalescing(retExpr1, retExpr2, comparedToNull, semanticModel, comparedIsNullInTrue))
                {
                    isNullCoalescing = true;
                }

                return true;
            }

            var expressionStatement2 = statement2 as ExpressionStatementSyntax;

            if (!(statement1 is ExpressionStatementSyntax expressionStatement1) || expressionStatement2 == null)
            {
                return false;
            }

            var expression1 = expressionStatement1.Expression.RemoveParentheses();
            var expression2 = expressionStatement2.Expression.RemoveParentheses();

            if (AreCandidateAssignments(expression1, expression2, comparedToNull, semanticModel,
                comparedIsNullInTrue, out isNullCoalescing))
            {
                return true;
            }

            if (comparedToNull != null &&
                CanExpressionBeNullCoalescing(expression1, expression2, comparedToNull, semanticModel, comparedIsNullInTrue))
            {
                isNullCoalescing = true;
                return true;
            }

            return AreCandidateInvocationsForTernary(expression1, expression2, semanticModel);
        }

        private static bool AreCandidateAssignments(ExpressionSyntax expression1, ExpressionSyntax expression2,
            ExpressionSyntax compared, SemanticModel semanticModel, bool comparedIsNullInTrue, out bool isNullCoalescing)
        {
            isNullCoalescing = false;
            var assignment1 = expression1 as AssignmentExpressionSyntax;
            var assignment2 = expression2 as AssignmentExpressionSyntax;
            var canBeSimplified =
                assignment1 != null &&
                assignment2 != null &&
                CSharpEquivalenceChecker.AreEquivalent(assignment1.Left, assignment2.Left) &&
                assignment1.Kind() == assignment2.Kind();

            if (!canBeSimplified)
            {
                return false;
            }

            if (!AreTypesCompatible(assignment1.Right, assignment2.Right, semanticModel))
            {
                return false;
            }

            if (compared != null &&
                CanExpressionBeNullCoalescing(assignment1.Right, assignment2.Right, compared, semanticModel, comparedIsNullInTrue))
            {
                isNullCoalescing = true;
            }

            return true;
        }

        internal static StatementSyntax ExtractSingleStatement(StatementSyntax statement)
        {
            if (!(statement is BlockSyntax block))
            {
                return statement;
            }

            return block.Statements.Count == 1
                ? block.Statements.First()
                : null;
        }

        private static bool AreCandidateInvocationsForNullCoalescing(ExpressionSyntax expression1, ExpressionSyntax expression2,
            ExpressionSyntax comparedToNull, SemanticModel semanticModel,
            bool comparedIsNullInTrue)
        {
            return AreCandidateInvocations(expression1, expression2, comparedToNull, semanticModel, comparedIsNullInTrue);
        }

        private static bool AreCandidateInvocationsForTernary(ExpressionSyntax expression1, ExpressionSyntax expression2,
            SemanticModel semanticModel)
        {
            return AreCandidateInvocations(expression1, expression2, null, semanticModel, comparedIsNullInTrue: false);
        }

        private static bool AreCandidateInvocations(ExpressionSyntax expression1, ExpressionSyntax expression2,
            ExpressionSyntax comparedToNull, SemanticModel semanticModel,
            bool comparedIsNullInTrue)
        {
            var methodCall2 = expression2 as InvocationExpressionSyntax;

            if (!(expression1 is InvocationExpressionSyntax methodCall1) || methodCall2 == null)
            {
                return false;
            }

            var methodSymbol1 = semanticModel.GetSymbolInfo(methodCall1).Symbol;
            var methodSymbol2 = semanticModel.GetSymbolInfo(methodCall2).Symbol;

            if (methodSymbol1 == null ||
                methodSymbol2 == null ||
                !methodSymbol1.Equals(methodSymbol2))
            {
                return false;
            }

            if (methodCall1.ArgumentList == null ||
                methodCall2.ArgumentList == null ||
                methodCall1.ArgumentList.Arguments.Count != methodCall2.ArgumentList.Arguments.Count)
            {
                return false;
            }

            var numberOfDifferences = 0;
            var numberOfComparisonsToCondition = 0;
            for (var i = 0; i < methodCall1.ArgumentList.Arguments.Count; i++)
            {
                var arg1 = methodCall1.ArgumentList.Arguments[i]?.Expression.RemoveParentheses();
                var arg2 = methodCall2.ArgumentList.Arguments[i]?.Expression.RemoveParentheses();

                if (!CSharpEquivalenceChecker.AreEquivalent(arg1, arg2))
                {
                    numberOfDifferences++;

                    if (comparedToNull != null)
                    {
                        var arg1IsComparedToNull = CSharpEquivalenceChecker.AreEquivalent(arg1, comparedToNull);
                        var arg2IsComparedToNull = CSharpEquivalenceChecker.AreEquivalent(arg2, comparedToNull);

                        if (arg1IsComparedToNull && !comparedIsNullInTrue)
                        {
                            numberOfComparisonsToCondition++;
                        }

                        if (arg2IsComparedToNull && comparedIsNullInTrue)
                        {
                            numberOfComparisonsToCondition++;
                        }
                    }

                    if (!AreTypesCompatible(arg1, arg2, semanticModel))
                    {
                        return false;
                    }
                }
                else
                {
                    if (comparedToNull != null && CSharpEquivalenceChecker.AreEquivalent(arg1, comparedToNull))
                    {
                        return false;
                    }
                }
            }

            return numberOfDifferences == 1 && (comparedToNull == null || numberOfComparisonsToCondition == 1);
        }

        private static bool CanExpressionBeNullCoalescing(ExpressionSyntax whenTrue, ExpressionSyntax whenFalse,
            ExpressionSyntax comparedToNull, SemanticModel semanticModel, bool comparedIsNullInTrue)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(whenTrue, comparedToNull))
            {
                return !comparedIsNullInTrue;
            }

            if (CSharpEquivalenceChecker.AreEquivalent(whenFalse, comparedToNull))
            {
                return comparedIsNullInTrue;
            }

            return AreCandidateInvocationsForNullCoalescing(whenTrue, whenFalse, comparedToNull,
                semanticModel, comparedIsNullInTrue);
        }

        internal static bool TryGetExpressionComparedToNull(ExpressionSyntax expression,
            out ExpressionSyntax compared, out bool comparedIsNullInTrue)
        {
            compared = null;
            comparedIsNullInTrue = false;
            if (!(expression is BinaryExpressionSyntax binary) ||
                !EqualsOrNotEquals.Contains(binary.Kind()))
            {
                return false;
            }

            comparedIsNullInTrue = binary.IsKind(SyntaxKind.EqualsExpression);

            if (CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.NullLiteralExpression))
            {
                compared = binary.Right;
                return true;
            }

            if (CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.NullLiteralExpression))
            {
                compared = binary.Left;
                return true;
            }

            return false;
        }

        private static bool ExpressionCanBeNull(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var expressionType = semanticModel.GetTypeInfo(expression).Type;
            return expressionType != null &&
                   (expressionType.IsReferenceType ||
                    expressionType.Is(KnownType.System_Nullable_T));
        }

        private static readonly ISet<SyntaxKind> EqualsOrNotEquals = new HashSet<SyntaxKind>
        {
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression
        };
    }
}
