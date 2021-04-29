/*
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
#else
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
#endif

namespace SonarAnalyzer.Extensions
{
    internal static class StatementSyntaxEstensions
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
    }
}
