/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
