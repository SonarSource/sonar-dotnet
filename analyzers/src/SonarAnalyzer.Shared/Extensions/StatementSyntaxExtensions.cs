﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;

#if CS
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#else
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
#endif

namespace SonarAnalyzer.Extensions
{
    internal static class StatementSyntaxExtensions
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
        public static StatementSyntax GetPrecedingStatement(this StatementSyntax currentStatement)
        {
            var previousStatement = currentStatement.GetPreviousStatement();
            if (previousStatement == null && currentStatement.SyntaxTree.HasCompilationUnitRoot) // this means that we might be in a top-level-statement
            {
                previousStatement = currentStatement.GetPreviousStatement(currentStatement.SyntaxTree.GetCompilationUnitRoot());
            }
            return previousStatement;
        }

        private static StatementSyntax GetPreviousStatement(this StatementSyntax currentStatement) =>
            currentStatement.Parent
                            .ChildNodes()
                            .OfType<StatementSyntax>()
                            .TakeWhile(x => x != currentStatement)
                            .LastOrDefault();

        /// <summary>
        /// Returns the statement before the statement given as input, in top level statements.
        /// </summary>
        private static StatementSyntax GetPreviousStatement(this StatementSyntax currentStatement, CompilationUnitSyntax rootNode) =>
            // The global statements, included in the top-level-statements, are siblings under one parent; the compilation Unit.
            rootNode.ChildNodes()
                    .Select(x => x.ChildNodes()
                                  .FirstOrDefault())
                    .Where(x => x != null)
                    .OfType<StatementSyntax>()
                    .TakeWhile(x => x != currentStatement)
                    .LastOrDefault();
#endif
    }
}
