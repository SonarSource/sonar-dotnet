/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.SymbolicExecution.CFG
{
    public class UsingEndBlock : SimpleBlock
    {
        public UsingStatementSyntax UsingStatement { get; }
        public IEnumerable<SyntaxToken> Identifiers { get; }

        public UsingEndBlock(UsingStatementSyntax usingStatement, Block successor)
            : base(successor)
        {
            UsingStatement = usingStatement;

            Identifiers = usingStatement.Declaration != null
                ? GetIdentifiers(usingStatement.Declaration)
                : GetIdentifiers(usingStatement.Expression);
        }

        private static IEnumerable<SyntaxToken> GetIdentifiers(VariableDeclarationSyntax declaration)
        {
            return declaration.Variables
                .Select(v => v.Identifier)
                .ToImmutableArray();
        }

        private static IEnumerable<SyntaxToken> GetIdentifiers(ExpressionSyntax expression)
        {
            var identifier = expression.RemoveParentheses() as IdentifierNameSyntax;
            return identifier != null
                ? ImmutableArray.Create(identifier.Identifier)
                : expression.DescendantNodesAndSelf()
                    .OfType<AssignmentExpressionSyntax>()
                    .Select(a => a.Left)
                    .OfType<IdentifierNameSyntax>()
                    .Select(i => i.Identifier)
                    .ToImmutableArray();
        }
    }
}
