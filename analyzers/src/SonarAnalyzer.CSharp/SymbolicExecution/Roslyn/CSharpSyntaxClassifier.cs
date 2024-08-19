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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.CSharp;

public sealed class CSharpSyntaxClassifier : SyntaxClassifierBase
{
    private static CSharpSyntaxClassifier instance;

    public static CSharpSyntaxClassifier Instance => instance ??= new();

    private CSharpSyntaxClassifier() { }

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
