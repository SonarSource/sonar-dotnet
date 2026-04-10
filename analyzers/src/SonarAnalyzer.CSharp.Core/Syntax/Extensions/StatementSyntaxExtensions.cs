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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class StatementSyntaxExtensions
{
    extension(StatementSyntax statement)
    {
        /// <summary>
        /// Returns the statement before the statement given as input.
        /// </summary>
        public StatementSyntax PrecedingStatement =>
            statement.SiblingStatements()
                .OfType<StatementSyntax>()
                .TakeWhile(x => x != statement)
                .LastOrDefault();

        /// <summary>
        /// Returns the statement after the statement given as input.
        /// </summary>
        public StatementSyntax FollowingStatement =>
            statement.SiblingStatements()
                .OfType<StatementSyntax>()
                .SkipWhile(x => x != statement)
                .Skip(1)
                .FirstOrDefault();

        public StatementSyntax FirstNonBlockStatement
        {
            get
            {
                var current = statement;
                while (current is BlockSyntax { } block)
                {
                    current = block.Statements.FirstOrDefault();
                }
                return current;
            }
        }

        private IEnumerable<SyntaxNode> SiblingStatements() =>
            statement.Parent is GlobalStatementSyntax
                ? statement.SyntaxTree
                    .GetCompilationUnitRoot()
                    .ChildNodes()
                    .OfType<GlobalStatementSyntax>()
                    .Select(x => x.Statement)
                : statement.Parent.ChildNodes();
    }
}
