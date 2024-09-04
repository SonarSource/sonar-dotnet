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

#if CS
namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;
#else
namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;
#endif

public static class StatementSyntaxExtensionsShared
{
    /// <summary>
    /// Returns all statements before the specified statement within the containing method.
    /// This method recursively traverses all parent blocks of the provided statement.
    /// </summary>
    public static IEnumerable<StatementSyntax> GetPreviousStatements(this StatementSyntax statement)
    {
        var previousStatements = statement.GetPreviousStatementsCurrentBlock();

        return statement.Parent is StatementSyntax parentStatement
            ? previousStatements.Union(GetPreviousStatements(parentStatement))
            : previousStatements;
    }

#if CS
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

    private static StatementSyntax GetPrecedingStatement(this StatementSyntax statement, IEnumerable<SyntaxNode> statementSiblingNodes) =>
        statementSiblingNodes.OfType<StatementSyntax>()
                             .TakeWhile(x => x != statement)
                             .LastOrDefault();
#endif
}
