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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class StatementSyntaxExtensions
{
    /// <summary>
    /// Returns the statement before the statement given as input.
    /// </summary>
    public static StatementSyntax GetPrecedingStatement(this StatementSyntax statement)
    {
        var siblings = statement.Parent is GlobalStatementSyntax
            ? statement.SyntaxTree
                .GetCompilationUnitRoot()
                .ChildNodes()
                .OfType<GlobalStatementSyntax>()
                .Select(x => x.Statement)
            : statement.Parent.ChildNodes();
        return statement.GetPrecedingStatement(siblings);
    }

    public static StatementSyntax FirstNonBlockStatement(this StatementSyntax statement)
    {
        while (statement is not null)
        {
            if (statement is not BlockSyntax { Statements: var blockStatements })
            {
                return statement;
            }
            statement = blockStatements.FirstOrDefault();
        }
        return null;
    }

    private static StatementSyntax GetPrecedingStatement(this StatementSyntax statement, IEnumerable<SyntaxNode> statementSiblingNodes) =>
        statementSiblingNodes.OfType<StatementSyntax>()
            .TakeWhile(x => x != statement)
            .LastOrDefault();
}
