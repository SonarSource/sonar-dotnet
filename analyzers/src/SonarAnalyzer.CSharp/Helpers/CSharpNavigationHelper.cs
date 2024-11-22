/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers;

internal static class CSharpNavigationHelper
{
    #region If

    public static IList<IfStatementSyntax> GetPrecedingIfsInConditionChain(this IfStatementSyntax ifStatement)
    {
        var ifList = new List<IfStatementSyntax>();
        var currentIf = ifStatement;

        while (currentIf.Parent.IsKind(SyntaxKind.ElseClause) &&
            currentIf.Parent.Parent.IsKind(SyntaxKind.IfStatement))
        {
            var precedingIf = (IfStatementSyntax)currentIf.Parent.Parent;
            ifList.Add(precedingIf);
            currentIf = precedingIf;
        }

        ifList.Reverse();
        return ifList;
    }

    public static IEnumerable<StatementSyntax> GetPrecedingStatementsInConditionChain(
        this IfStatementSyntax ifStatement)
    {
        return GetPrecedingIfsInConditionChain(ifStatement).Select(i => i.Statement);
    }

    public static IEnumerable<ExpressionSyntax> GetPrecedingConditionsInConditionChain(
        this IfStatementSyntax ifStatement)
    {
        return GetPrecedingIfsInConditionChain(ifStatement).Select(i => i.Condition);
    }

    #endregion If

    #region Switch

    public static IEnumerable<SwitchSectionSyntax> GetPrecedingSections(this SwitchSectionSyntax caseStatement)
    {
        if (caseStatement == null)
        {
            return new SwitchSectionSyntax[0];
        }

        var switchStatement = (SwitchStatementSyntax)caseStatement.Parent;
        var currentSectionIndex = switchStatement.Sections.IndexOf(caseStatement);
        return switchStatement.Sections.Take(currentSectionIndex);
    }

    #endregion Switch
}
