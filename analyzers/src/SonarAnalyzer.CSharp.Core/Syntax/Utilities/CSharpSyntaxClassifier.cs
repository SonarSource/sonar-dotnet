/*
 * Copyright (C) 2015-2025 SonarSource Sàrl
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using SonarAnalyzer.CFG.Syntax.Utilities;

namespace SonarAnalyzer.CSharp.Core.Syntax.Utilities;

public sealed class CSharpSyntaxClassifier : SyntaxClassifierBase
{
    private static CSharpSyntaxClassifier instance;

    public static CSharpSyntaxClassifier Instance => instance ??= new();

    private CSharpSyntaxClassifier() { }

    public override SyntaxNode MemberAccessExpression(SyntaxNode node) =>
        (node as MemberAccessExpressionSyntax)?.Expression;

    protected override bool IsStatement(SyntaxNode node) =>
            node is StatementSyntax;

    protected override SyntaxNode ParentLoopCondition(SyntaxNode node) =>
        node.Parent switch
        {
            DoStatementSyntax doStatement => doStatement.Condition,
            ForStatementSyntax forStatement => forStatement.Condition,
            ForEachStatementSyntax forEachStatement => forEachStatement.Expression,
            WhileStatementSyntax whileStatement => whileStatement.Condition,
            _ when node.Parent.IsKind(SyntaxKindEx.ForEachVariableStatement) => ((ForEachVariableStatementSyntaxWrapper)node.Parent).Expression,
            _ => null
        };

    protected override bool IsCfgBoundary(SyntaxNode node) =>
        node is LambdaExpressionSyntax || node.IsKind(SyntaxKindEx.LocalFunctionStatement);
}
