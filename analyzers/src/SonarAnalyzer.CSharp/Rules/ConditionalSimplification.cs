﻿/*
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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConditionalSimplification : SonarDiagnosticAnalyzer
{
    internal const string SimplifiedOperatorKey = "SimplifiedOperator";
    internal const string IsCoalesceAssignmentSupportedKey = "IsNullCoalesceAssignmentSupported";
    internal const string DiagnosticId = "S3240";
    private const string MessageFormat = "Use the '{0}' operator here.";
    private const string MessageMultipleNegation = "Simplify negation here.";
    private const string CoalesceAssignmentOp = "??=";
    private const string CoalesceOp = "??";
    private const string TernaryOp = "?:";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    private static readonly DiagnosticDescriptor RuleMultipleNegation = DescriptorFactory.Create(DiagnosticId, MessageMultipleNegation);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule, RuleMultipleNegation);

    internal static bool IsCoalesceAssignmentCandidate(SyntaxNode conditional, ExpressionSyntax comparedToNull) =>
        conditional?.GetFirstNonParenthesizedParent() is AssignmentExpressionSyntax parentAssignment
        && CSharpEquivalenceChecker.AreEquivalent(parentAssignment.Left, comparedToNull);

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
        context.RegisterNodeAction(CheckConditionalExpression, SyntaxKind.ConditionalExpression);
        context.RegisterNodeAction(CheckIfStatement, SyntaxKind.IfStatement);
        context.RegisterNodeAction(CheckCoalesceExpression, SyntaxKind.CoalesceExpression);
        context.RegisterNodeAction(CheckNotPattern, SyntaxKindEx.NotPattern);
    }

    private static void CheckNotPattern(SonarSyntaxNodeReportingContext context)
    {
        var wrapper = (UnaryPatternSyntaxWrapper)context.Node;
        if (wrapper.Pattern.IsNot()
            && GetNegationRoot(context.Node) is var negationRoot
            && negationRoot == context.Node)
        {
            context.ReportIssue(RuleMultipleNegation, negationRoot);
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

    private static void CheckCoalesceExpression(SonarSyntaxNodeReportingContext context)
    {
        if (context.Node.GetFirstNonParenthesizedParent() is AssignmentExpressionSyntax assignment
            && !assignment.Parent.IsKind(SyntaxKind.ObjectInitializerExpression)
            && context.Compilation.IsCoalesceAssignmentSupported())
        {
            var left = ((BinaryExpressionSyntax)context.Node).Left.RemoveParentheses();
            if (CSharpEquivalenceChecker.AreEquivalent(assignment.Left.RemoveParentheses(), left))
            {
                context.ReportIssue(Rule, assignment.GetLocation(), BuildCodeFixProperties(context), CoalesceAssignmentOp);
            }
        }
    }

    private static void CheckIfStatement(SonarSyntaxNodeReportingContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;
        if (ifStatement.Parent is ElseClauseSyntax)
        {
            return;
        }

        var whenTrue = ExtractSingleStatement(ifStatement.Statement);
        var whenFalse = ExtractSingleStatement(ifStatement.Else?.Statement);
        if (whenTrue is null
            || (ifStatement.Else is not null && whenFalse is null)
            || (whenFalse is not null && CSharpEquivalenceChecker.AreEquivalent(whenTrue, whenFalse)))
        {
            // Equivalence handled by S1871, <see cref="ConditionalStructureSameImplementation"/>
            return;
        }

        var possiblyCoalescing = ifStatement.Condition.TryGetExpressionComparedToNull(out var comparedToNull, out var comparedIsNullInTrue)
                                 && comparedToNull.CanBeNull(context.Model);

        if (CanBeSimplified(context, whenTrue, whenFalse, possiblyCoalescing ? comparedToNull : null, context.Model, comparedIsNullInTrue, out var simplifiedOperator))
        {
            context.ReportIssue(Rule, ifStatement.IfKeyword, BuildCodeFixProperties(context, simplifiedOperator), simplifiedOperator);
        }
    }

    private static void CheckConditionalExpression(SonarSyntaxNodeReportingContext context)
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

        if (condition.TryGetExpressionComparedToNull(out var comparedToNull, out var comparedIsNullInTrue)
            && comparedToNull.CanBeNull(context.Model)
            && CanExpressionBeCoalescing(whenTrue, whenFalse, comparedToNull, context.Model, comparedIsNullInTrue))
        {
            if (context.Compilation.IsCoalesceAssignmentSupported() && IsCoalesceAssignmentCandidate(conditional, comparedToNull))
            {
                context.ReportIssue(Rule, conditional.GetFirstNonParenthesizedParent(), BuildCodeFixProperties(context), CoalesceAssignmentOp);
            }
            else
            {
                context.ReportIssue(Rule, conditional, BuildCodeFixProperties(context), CoalesceOp);
            }
        }
    }

    private static bool AreTypesCompatible(ExpressionSyntax first, ExpressionSyntax second, SemanticModel model, ITypeSymbol targetType = null)
    {
        if (first is AnonymousFunctionExpressionSyntax || second is AnonymousFunctionExpressionSyntax)
        {
            return false;
        }

        var firstType = model.GetTypeInfo(first).Type;
        var secondType = model.GetTypeInfo(second).Type;

        if (firstType is IErrorTypeSymbol || secondType is IErrorTypeSymbol)
        {
            return false;
        }

        if (IsNullAndValueType(firstType, secondType) || IsNullAndValueType(secondType, firstType))
        {
            return false;
        }

        if (firstType is null || secondType is null)
        {
            return true;
        }

        if (targetType is not null && model.Compilation.IsTargetTypeConditionalSupported())
        {
            return firstType.DerivesFrom(targetType) && secondType.DerivesFrom(targetType);
        }

        return firstType.Equals(secondType);
    }

    private static bool IsNullAndValueType(ITypeSymbol typeNull, ITypeSymbol typeValue) =>
        typeNull is null && typeValue is { IsValueType: true };

    private static bool CanBeSimplified(SonarSyntaxNodeReportingContext context,
                                        StatementSyntax statement1,
                                        StatementSyntax statement2,
                                        SyntaxNode comparedToNull,
                                        SemanticModel model,
                                        bool comparedIsNullInTrue,
                                        out string simplifiedOperator)
    {
        simplifiedOperator = TernaryOp;

        if (statement1 is ReturnStatementSyntax return1 && statement2 is ReturnStatementSyntax return2)
        {
            var retExpr1 = return1.Expression.RemoveParentheses();
            var retExpr2 = return2.Expression.RemoveParentheses();

            if (IsConditionalStructure(retExpr1)
                || IsConditionalStructure(retExpr2)
                || !AreTypesCompatible(return1.Expression, return2.Expression, model))
            {
                return false;
            }
            if (comparedToNull is not null && CanExpressionBeCoalescing(retExpr1, retExpr2, comparedToNull, model, comparedIsNullInTrue))
            {
                simplifiedOperator = CoalesceOp;
            }
            return true;
        }

        var expressionStatement2 = statement2 as ExpressionStatementSyntax;
        if ((statement1 is not ExpressionStatementSyntax) || (statement2 is not null && expressionStatement2 is null))
        {
            return false;
        }

        var expression1 = ((ExpressionStatementSyntax)statement1).Expression.RemoveParentheses();
        if (statement2 is null)
        {
            simplifiedOperator = context.Compilation.IsCoalesceAssignmentSupported() ? CoalesceAssignmentOp : CoalesceOp;
            return expression1 is AssignmentExpressionSyntax assignment
                && comparedIsNullInTrue
                && comparedToNull is not null
                && CSharpEquivalenceChecker.AreEquivalent(assignment.Left, comparedToNull);
        }

        if (IsConditionalStructure(statement1) || IsConditionalStructure(statement2))
        {
            return false;
        }

        var expression2 = expressionStatement2.Expression.RemoveParentheses();
        if (AreCandidateAssignments(expression1, expression2, comparedToNull, model, comparedIsNullInTrue, out simplifiedOperator))
        {
            return true;
        }
        else if (comparedToNull is not null && CanExpressionBeCoalescing(expression1, expression2, comparedToNull, model, comparedIsNullInTrue))
        {
            simplifiedOperator = CoalesceOp;
            return true;
        }
        else
        {
            return AreCandidateInvocations(expression1, expression2, null, model, comparedIsNullInTrue: false);
        }
    }

    private static bool AreCandidateAssignments(ExpressionSyntax expression1,
                                                ExpressionSyntax expression2,
                                                SyntaxNode compared,
                                                SemanticModel model,
                                                bool comparedIsNullInTrue,
                                                out string simplifiedOperator)
    {
        simplifiedOperator = TernaryOp;
        var assignment1 = expression1 as AssignmentExpressionSyntax;
        var assignment2 = expression2 as AssignmentExpressionSyntax;
        var canBeSimplified = assignment1 is not null
                              && assignment2 is not null
                              && CSharpEquivalenceChecker.AreEquivalent(assignment1.Left, assignment2.Left)
                              && assignment1.Kind() == assignment2.Kind();

        if (!canBeSimplified || !AreTypesCompatible(assignment1.Right, assignment2.Right, model, model.GetTypeInfo(assignment1.Left).Type))
        {
            return false;
        }

        if (compared is not null && CanExpressionBeCoalescing(assignment1.Right, assignment2.Right, compared, model, comparedIsNullInTrue))
        {
            simplifiedOperator = CoalesceOp;
        }

        return true;
    }

    private static bool AreCandidateInvocations(ExpressionSyntax firstExpression,
                                                ExpressionSyntax secondExpression,
                                                SyntaxNode comparedToNull,
                                                SemanticModel model,
                                                bool comparedIsNullInTrue)
    {
        if (firstExpression is not InvocationExpressionSyntax firstInvocation
            || secondExpression is not InvocationExpressionSyntax secondInvocation
            || firstInvocation.ArgumentList.Arguments.Count != secondInvocation.ArgumentList.Arguments.Count
            || !CSharpEquivalenceChecker.AreEquivalent(firstInvocation.Expression, secondInvocation.Expression)
            || model.GetSymbolInfo(firstInvocation).Symbol is not { } firstSymbol
            || model.GetSymbolInfo(secondInvocation).Symbol is not { } secondSymbol
            || !firstSymbol.Equals(secondSymbol))
        {
            return false;
        }

        var numberOfDifferences = 0;
        var numberOfComparisonsToCondition = 0;
        for (var i = 0; i < firstInvocation.ArgumentList.Arguments.Count; i++)
        {
            var arg1 = firstInvocation.ArgumentList.Arguments[i].Expression.RemoveParentheses();
            var arg2 = secondInvocation.ArgumentList.Arguments[i].Expression.RemoveParentheses();
            if (!CSharpEquivalenceChecker.AreEquivalent(arg1, arg2))
            {
                numberOfDifferences++;
                if (comparedToNull is not null)
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
                if (!AreTypesCompatible(arg1, arg2, model))
                {
                    return false;
                }
            }
            else
            {
                if (comparedToNull is not null && CSharpEquivalenceChecker.AreEquivalent(arg1, comparedToNull))
                {
                    return false;
                }
            }
        }
        return numberOfDifferences == 1 && (comparedToNull is null || numberOfComparisonsToCondition is 1);
    }

    private static bool CanExpressionBeCoalescing(ExpressionSyntax whenTrue, ExpressionSyntax whenFalse, SyntaxNode comparedToNull, SemanticModel model, bool comparedIsNullInTrue)
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
            return AreCandidateInvocations(whenTrue, whenFalse, comparedToNull, model, comparedIsNullInTrue);
        }
    }

    private static ImmutableDictionary<string, string> BuildCodeFixProperties(SonarSyntaxNodeReportingContext c, string simplifiedOperator = null)
    {
        var ret = new Dictionary<string, string> { { IsCoalesceAssignmentSupportedKey, c.Compilation.IsCoalesceAssignmentSupported().ToString() } };
        if (simplifiedOperator is not null)
        {
            ret.Add(SimplifiedOperatorKey, simplifiedOperator);
        }
        return ret.ToImmutableDictionary();
    }

    private static bool IsConditionalStructure(SyntaxNode node) =>
        node.DescendantNodesAndSelf().Any(x => x.Kind() is SyntaxKind.ConditionalExpression or SyntaxKindEx.SwitchExpression);
}
