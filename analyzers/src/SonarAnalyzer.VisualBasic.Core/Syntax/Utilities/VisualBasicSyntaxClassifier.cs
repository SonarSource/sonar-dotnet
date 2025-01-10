/*
 * Copyright (C) 2015-2025 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using SonarAnalyzer.CFG.Syntax.Utilities;

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;

public sealed class VisualBasicSyntaxClassifier : SyntaxClassifierBase
{
    private static VisualBasicSyntaxClassifier instance;

    public static VisualBasicSyntaxClassifier Instance => instance ??= new();

    private VisualBasicSyntaxClassifier() { }

    public override SyntaxNode MemberAccessExpression(SyntaxNode node) =>
        (node as MemberAccessExpressionSyntax)?.Expression;

    protected override bool IsStatement(SyntaxNode node) =>
        node is StatementSyntax;

    protected override SyntaxNode ParentLoopCondition(SyntaxNode node) =>
        node.Parent switch
        {
            DoStatementSyntax doStatement => doStatement.WhileOrUntilClause,
            LoopStatementSyntax loopStatement => loopStatement.WhileOrUntilClause,
            ForBlockSyntax forBlock => forBlock.ForStatement,
            ForEachStatementSyntax foreachStatement => foreachStatement.Expression,
            WhileStatementSyntax whileStatement => whileStatement.Condition,
            _ => null
        };

    protected override bool IsCfgBoundary(SyntaxNode node) =>
        node is LambdaExpressionSyntax;
}
