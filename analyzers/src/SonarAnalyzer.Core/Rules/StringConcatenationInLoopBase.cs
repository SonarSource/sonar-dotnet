/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Rules;

public abstract class StringConcatenationInLoopBase<TSyntaxKind, TAssignmentExpression, TBinaryExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TAssignmentExpression : SyntaxNode
    where TBinaryExpression : SyntaxNode
{
    protected const string DiagnosticId = "S1643";

    protected abstract TSyntaxKind[] CompoundAssignmentKinds { get; }
    protected abstract ISet<TSyntaxKind> ExpressionConcatenationKinds { get; }
    protected abstract ISet<TSyntaxKind> LoopKinds { get; }
    protected abstract SyntaxNode LeftMostExpression(SyntaxNode expression);

    protected override string MessageFormat => "Use a StringBuilder instead.";

    protected StringConcatenationInLoopBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckSimpleAssignment, Language.SyntaxKind.SimpleAssignment);
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckCompoundAssignment, CompoundAssignmentKinds);
    }

    private void CheckSimpleAssignment(SonarSyntaxNodeReportingContext context)
    {
        var assignment = (TAssignmentExpression)context.Node;

        if (Language.Syntax.AssignmentRight(assignment) is TBinaryExpression { } rightExpression
            && Language.Syntax.IsAnyKind(rightExpression, ExpressionConcatenationKinds)
            && Language.Syntax.AssignmentLeft(assignment) is var assigned
            && IsIdentifierOnTheRight(assigned, rightExpression)
            && IsSystemString(assigned, context.Model)
            && LeftMostExpression(assigned) is { } expressionToCheck
            && AreNotDefinedInTheSameLoop(expressionToCheck, assignment, context.Model))
        {
            context.ReportIssue(SupportedDiagnostics[0], assignment);
        }
    }

    private void CheckCompoundAssignment(SonarSyntaxNodeReportingContext context)
    {
        var addAssignment = (TAssignmentExpression)context.Node;
        if (Language.Syntax.AssignmentLeft(addAssignment) is { } expression
            && IsSystemString(expression, context.Model)
            && LeftMostExpression(expression) is { } expressionToCheck
            && AreNotDefinedInTheSameLoop(expressionToCheck, addAssignment, context.Model))
        {
            context.ReportIssue(SupportedDiagnostics[0], addAssignment);
        }
    }

    private bool IsIdentifierOnTheRight(SyntaxNode identifier, SyntaxNode expression)
    {
        while (expression is TBinaryExpression && Language.Syntax.IsAnyKind(expression, ExpressionConcatenationKinds))
        {
            var left = Language.Syntax.BinaryExpressionLeft(expression);
            var right = Language.Syntax.BinaryExpressionRight(expression);

            if (Language.Syntax.AreEquivalent(left, identifier) || Language.Syntax.AreEquivalent(right, identifier))
            {
                return true;
            }
            // No need to recurse into the right branch as the only useful concatenation is flat `"a" + "b" + s`
            // `"a" + (s  + "b")` seems not worth to support.
            expression = left;
        }
        return false;
    }

    private static bool IsSystemString(SyntaxNode node, SemanticModel model) =>
        node.IsKnownType(KnownType.System_String, model);

    private SyntaxNode NearestLoop(SyntaxNode node) =>
        node.AncestorsAndSelf().FirstOrDefault(x => Language.Syntax.IsAnyKind(x, LoopKinds));

    private bool AreNotDefinedInTheSameLoop(SyntaxNode expression, SyntaxNode assignment, SemanticModel model) =>
        NearestLoop(assignment) is { } nearestLoopForConcatenation
        && !(model.GetSymbolInfo(expression).Symbol is { } symbol
                && symbol.GetFirstSyntaxRef() is { } declaration
                && NearestLoop(declaration) is { } nearestLoop
                && Language.Syntax.AreEquivalent(nearestLoop, nearestLoopForConcatenation));
}
