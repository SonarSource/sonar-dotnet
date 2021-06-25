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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ConditionalSimplification : SonarDiagnosticAnalyzer
    {
        internal const string SimplifiedOperatorKey = "SimplifiedOperator";
        internal const string IsCoalesceAssignmentSupportedKey = "IsNullCoalesceAssignmentSupported";
        internal const string DiagnosticId = "S3240";
        private const string MessageFormat = "Use the '{0}' operator here.";
        private const string MessageMultipleNegation = "Simplify negation here.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly DiagnosticDescriptor RuleMultipleNegation = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageMultipleNegation, RspecStrings.ResourceManager);

        private static readonly ISet<SyntaxKind> EqualsOrNotEquals = new HashSet<SyntaxKind>
        {
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        internal static bool IsCoalesceAssignmentCandidate(SyntaxNode conditional, ExpressionSyntax comparedToNull) =>
            conditional?.GetFirstNonParenthesizedParent() is AssignmentExpressionSyntax parentAssignment
            && CSharpEquivalenceChecker.AreEquivalent(parentAssignment.Left, comparedToNull);

        internal static bool TryGetExpressionComparedToNull(ExpressionSyntax expression, out ExpressionSyntax compared, out bool comparedIsNullInTrue)
        {
            compared = null;
            comparedIsNullInTrue = false;
            if (expression.RemoveParentheses() is BinaryExpressionSyntax binary && EqualsOrNotEquals.Contains(binary.Kind()))
            {
                comparedIsNullInTrue = binary.IsKind(SyntaxKind.EqualsExpression);
                if (CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.NullLiteralExpression))
                {
                    compared = binary.Right;
                    return true;
                }
                else if (CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.NullLiteralExpression))
                {
                    compared = binary.Left;
                    return true;
                }
            }

            if (IsPatternExpressionSyntaxWrapper.IsInstance(expression.RemoveParentheses()))
            {
                var isPatternWrapper = (IsPatternExpressionSyntaxWrapper)expression.RemoveParentheses();
                if (isPatternWrapper.IsNotNull())
                {
                    comparedIsNullInTrue = false;
                    compared = isPatternWrapper.Expression;
                    return true;
                }
                else if (isPatternWrapper.IsNull())
                {
                    comparedIsNullInTrue = true;
                    compared = isPatternWrapper.Expression;
                    return true;
                }
            }

            return false;
        }

        internal static StatementSyntax ExtractSingleStatement(StatementSyntax statement)
        {
            if (statement is BlockSyntax block)
            {
                return block.Statements.Count == 1 ? block.Statements.First() : null;
            }
            return statement;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckConditionalExpression, SyntaxKind.ConditionalExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckIfStatement, SyntaxKind.IfStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckCoalesceExpression, SyntaxKind.CoalesceExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckNotPattern, SyntaxKindEx.NotPattern);
        }

        private static void CheckNotPattern(SyntaxNodeAnalysisContext context)
        {
            var wrapper = (UnaryPatternSyntaxWrapper)context.Node;
            if (wrapper.Pattern.IsNot()
                && GetNegationRoot(context.Node) is var negationRoot
                && negationRoot == context.Node)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(RuleMultipleNegation, negationRoot.GetLocation()));
            }

            static SyntaxNode GetNegationRoot(SyntaxNode node)
            {
                while (node.Parent.Kind() == SyntaxKindEx.NotPattern)
                {
                    node = node.Parent;
                }
                return node;
            }
        }

        private static void CheckCoalesceExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.GetFirstNonParenthesizedParent() is AssignmentExpressionSyntax assignment
                && context.Compilation.IsCoalesceAssignmentSupported())
            {
                var left = ((BinaryExpressionSyntax)context.Node).Left.RemoveParentheses();
                if (CSharpEquivalenceChecker.AreEquivalent(assignment.Left.RemoveParentheses(), left))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, assignment.GetLocation(), BuildCodeFixProperties(context), "??="));
                }
            }
        }

        private static void CheckIfStatement(SyntaxNodeAnalysisContext context)
        {
            var ifStatement = (IfStatementSyntax)context.Node;
            if (ifStatement.Parent is ElseClauseSyntax)
            {
                return;
            }

            var whenTrue = ExtractSingleStatement(ifStatement.Statement);
            var whenFalse = ExtractSingleStatement(ifStatement.Else?.Statement);
            if (whenTrue == null
                || (ifStatement.Else != null && whenFalse == null)
                || (whenFalse != null && CSharpEquivalenceChecker.AreEquivalent(whenTrue, whenFalse)))
            {
                // Equivalence handled by S1871, <see cref="ConditionalStructureSameImplementation"/>
                return;
            }

            var possiblyCoalescing = TryGetExpressionComparedToNull(ifStatement.Condition, out var comparedToNull, out var comparedIsNullInTrue)
                                     && comparedToNull.CanBeNull(context.SemanticModel);

            if (CanBeSimplified(context, whenTrue, whenFalse, possiblyCoalescing ? comparedToNull : null, context.SemanticModel, comparedIsNullInTrue, out var simplifiedOperator))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule,
                                                                     ifStatement.IfKeyword.GetLocation(),
                                                                     BuildCodeFixProperties(context, simplifiedOperator),
                                                                     simplifiedOperator));
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
                // handled by S2758, <see cref="TernaryOperatorPointless"/>
                return;
            }

            if (TryGetExpressionComparedToNull(condition, out var comparedToNull, out var comparedIsNullInTrue)
                && comparedToNull.CanBeNull(context.SemanticModel)
                && CanExpressionBeCoalescing(whenTrue, whenFalse, comparedToNull, context.SemanticModel, comparedIsNullInTrue))
            {
                if (context.Compilation.IsCoalesceAssignmentSupported() && IsCoalesceAssignmentCandidate(conditional, comparedToNull))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, conditional.GetFirstNonParenthesizedParent().GetLocation(), BuildCodeFixProperties(context), "??="));
                }
                else
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, conditional.GetLocation(), BuildCodeFixProperties(context), "??"));
                }
            }
        }

        private static bool AreTypesCompatible(ExpressionSyntax first, ExpressionSyntax second, SemanticModel semanticModel, ITypeSymbol targetType = null)
        {
            if (first is AnonymousFunctionExpressionSyntax || second is AnonymousFunctionExpressionSyntax)
            {
                return false;
            }

            var firstType = semanticModel.GetTypeInfo(first).Type;
            var secondType = semanticModel.GetTypeInfo(second).Type;

            if (firstType is IErrorTypeSymbol || secondType is IErrorTypeSymbol)
            {
                return false;
            }

            if (IsNullAndValueType(firstType, secondType) || IsNullAndValueType(secondType, firstType))
            {
                return false;
            }

            if (firstType == null || secondType == null)
            {
                return true;
            }

            if (targetType != null && semanticModel.Compilation.IsTargetTypeConditionalSupported())
            {
                return firstType.DerivesFrom(targetType) && secondType.DerivesFrom(targetType);
            }

            return firstType.Equals(secondType);
        }

        private static bool IsNullAndValueType(ITypeSymbol typeNull, ITypeSymbol typeValue) =>
            typeNull == null && typeValue is {IsValueType: true};

        private static bool CanBeSimplified(SyntaxNodeAnalysisContext context,
                                            StatementSyntax statement1,
                                            StatementSyntax statement2,
                                            SyntaxNode comparedToNull,
                                            SemanticModel semanticModel,
                                            bool comparedIsNullInTrue,
                                            out string simplifiedOperator)
        {
            simplifiedOperator = "?:";

            if (statement1 is ReturnStatementSyntax return1
                && statement2 is ReturnStatementSyntax return2)
            {
                var retExpr1 = return1.Expression.RemoveParentheses();
                var retExpr2 = return2.Expression.RemoveParentheses();

                if (IsConditionalStructure(retExpr1)
                    || IsConditionalStructure(retExpr2)
                    || !AreTypesCompatible(return1.Expression, return2.Expression, semanticModel))
                {
                    return false;
                }
                if (comparedToNull != null && CanExpressionBeCoalescing(retExpr1, retExpr2, comparedToNull, semanticModel, comparedIsNullInTrue))
                {
                    simplifiedOperator = "??";
                }
                return true;
            }

            var expressionStatement2 = statement2 as ExpressionStatementSyntax;
            if (!(statement1 is ExpressionStatementSyntax expressionStatement1) || (statement2 != null && expressionStatement2 == null))
            {
                return false;
            }

            var expression1 = expressionStatement1.Expression.RemoveParentheses();
            if (statement2 == null)
            {
                simplifiedOperator = context.Compilation.IsCoalesceAssignmentSupported() ? "??=" : "??";
                return expression1 is AssignmentExpressionSyntax assignment
                    && comparedIsNullInTrue
                    && comparedToNull != null
                    && CSharpEquivalenceChecker.AreEquivalent(assignment.Left, comparedToNull);
            }

            var expression2 = expressionStatement2.Expression.RemoveParentheses();
            if (AreCandidateAssignments(expression1, expression2, comparedToNull, semanticModel, comparedIsNullInTrue, out simplifiedOperator))
            {
                return true;
            }
            else if (comparedToNull != null && CanExpressionBeCoalescing(expression1, expression2, comparedToNull, semanticModel, comparedIsNullInTrue))
            {
                simplifiedOperator = "??";
                return true;
            }
            else
            {
                return AreCandidateInvocations(expression1, expression2, null, semanticModel, comparedIsNullInTrue: false);
            }
        }

        private static bool AreCandidateAssignments(ExpressionSyntax expression1,
                                                    ExpressionSyntax expression2,
                                                    SyntaxNode compared,
                                                    SemanticModel semanticModel,
                                                    bool comparedIsNullInTrue,
                                                    out string simplifiedOperator)
        {
            simplifiedOperator = "?:";
            var assignment1 = expression1 as AssignmentExpressionSyntax;
            var assignment2 = expression2 as AssignmentExpressionSyntax;
            var canBeSimplified = assignment1 != null
                                  && assignment2 != null
                                  && CSharpEquivalenceChecker.AreEquivalent(assignment1.Left, assignment2.Left)
                                  && assignment1.Kind() == assignment2.Kind();

            if (!canBeSimplified || !AreTypesCompatible(assignment1.Right, assignment2.Right, semanticModel, semanticModel.GetTypeInfo(assignment1.Left).Type))
            {
                return false;
            }

            if (compared != null && CanExpressionBeCoalescing(assignment1.Right, assignment2.Right, compared, semanticModel, comparedIsNullInTrue))
            {
                simplifiedOperator = "??";
            }

            return true;
        }

        private static bool AreCandidateInvocations(ExpressionSyntax expression1,
                                                    ExpressionSyntax expression2,
                                                    SyntaxNode comparedToNull,
                                                    SemanticModel semanticModel,
                                                    bool comparedIsNullInTrue)
        {
            if (!(expression1 is InvocationExpressionSyntax methodCall1) || !(expression2 is InvocationExpressionSyntax methodCall2))
            {
                return false;
            }

            var methodSymbol1 = semanticModel.GetSymbolInfo(methodCall1).Symbol;
            var methodSymbol2 = semanticModel.GetSymbolInfo(methodCall2).Symbol;

            if (methodSymbol1 == null || methodSymbol2 == null || !methodSymbol1.Equals(methodSymbol2))
            {
                return false;
            }

            if (methodCall1.ArgumentList == null
                || methodCall2.ArgumentList == null
                || methodCall1.ArgumentList.Arguments.Count != methodCall2.ArgumentList.Arguments.Count)
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

        private static bool CanExpressionBeCoalescing(ExpressionSyntax whenTrue, ExpressionSyntax whenFalse, SyntaxNode comparedToNull, SemanticModel semanticModel, bool comparedIsNullInTrue)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(whenTrue, comparedToNull))
            {
                return !comparedIsNullInTrue;
            }
            else if (CSharpEquivalenceChecker.AreEquivalent(whenFalse, comparedToNull))
            {
                return comparedIsNullInTrue;
            }
            else
            {
                return AreCandidateInvocations(whenTrue, whenFalse, comparedToNull, semanticModel, comparedIsNullInTrue);
            }
        }

        private static ImmutableDictionary<string, string> BuildCodeFixProperties(SyntaxNodeAnalysisContext c, string simplifiedOperator = null)
        {
            var ret = new Dictionary<string, string> {{IsCoalesceAssignmentSupportedKey, c.Compilation.IsCoalesceAssignmentSupported().ToString()}};
            if (simplifiedOperator != null)
            {
                ret.Add(SimplifiedOperatorKey, simplifiedOperator);
            }
            return ret.ToImmutableDictionary();
        }

        private static bool IsConditionalStructure(SyntaxNode syntaxNode) =>
            syntaxNode.DescendantNodesAndSelf()
                      .Any(node => node.IsAnyKind(SyntaxKind.ConditionalExpression, SyntaxKindEx.SwitchExpression));
    }
}
