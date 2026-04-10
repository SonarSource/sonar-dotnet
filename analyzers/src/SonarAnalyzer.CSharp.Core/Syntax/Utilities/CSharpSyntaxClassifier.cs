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
