/*
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

namespace SonarAnalyzer.CSharp.Syntax.Extensions;

internal static class IfStatementExtensions
{
    public static IList<IfStatementSyntax> PrecedingIfsInConditionChain(this IfStatementSyntax ifStatement)
    {
        var ifStatements = new List<IfStatementSyntax>();
        while (ifStatement.Parent.IsKind(SyntaxKind.ElseClause) && ifStatement.Parent.Parent.IsKind(SyntaxKind.IfStatement))
        {
            var precedingIf = (IfStatementSyntax)ifStatement.Parent.Parent;
            ifStatements.Add(precedingIf);
            ifStatement = precedingIf;
        }
        ifStatements.Reverse();
        return ifStatements;
    }

    public static IEnumerable<StatementSyntax> PrecedingStatementsInConditionChain(this IfStatementSyntax ifStatement) =>
        ifStatement.PrecedingIfsInConditionChain().Select(x => x.Statement);

    public static IEnumerable<ExpressionSyntax> PrecedingConditionsInConditionChain(this IfStatementSyntax ifStatement) =>
        ifStatement.PrecedingIfsInConditionChain().Select(x => x.Condition);
}
