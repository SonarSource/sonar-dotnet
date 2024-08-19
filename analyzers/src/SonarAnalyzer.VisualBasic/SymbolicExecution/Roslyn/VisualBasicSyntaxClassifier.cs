/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.VisualBasic;

public sealed class VisualBasicSyntaxClassifier : SyntaxClassifierBase
{
    private static VisualBasicSyntaxClassifier instance;

    public static VisualBasicSyntaxClassifier Instance => instance ??= new();

    private VisualBasicSyntaxClassifier() { }

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
