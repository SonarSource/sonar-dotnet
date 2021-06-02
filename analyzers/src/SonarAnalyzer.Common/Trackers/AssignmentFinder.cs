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

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public abstract class AssignmentFinder
    {
        protected abstract SyntaxNode GetTopMostContainingMethod(SyntaxNode node);
        protected abstract bool IsAssignmentToIdentifier(SyntaxNode node, string identifierName, bool anyAssignmentKind, out SyntaxNode rightExpression);
        protected abstract bool IsIdentifierDeclaration(SyntaxNode node, string identifierName, out SyntaxNode initializer);
        protected abstract bool IsLoop(SyntaxNode node);

        public SyntaxNode FindLinearPrecedingAssignmentExpression(string identifierName, SyntaxNode current, Func<SyntaxNode> defaultValue = null)
        {
            var method = GetTopMostContainingMethod(current);
            while (current != method && current?.Parent != null)
            {
                if (IsLoop(current) && ContainsNestedAssignmentToIdentifier(current))
                {
                    return null; // There's asignment inside this loop, value can be altered by each iteration
                }

                foreach (var statement in current.Parent.ChildNodes().TakeWhile(x => x != current).Reverse())
                {
                    if (IsAssignmentToIdentifier(statement, identifierName, false, out var right))
                    {
                        return right;
                    }
                    else if (IsIdentifierDeclaration(statement, identifierName, out var initializer))
                    {
                        return initializer;
                    }
                    else if (ContainsNestedAssignmentToIdentifier(statement))
                    {
                        return null; // Assignment inside nested statement (if, try, for, ...)
                    }
                }
                current = current.Parent;
            }
            return defaultValue == null ? null : defaultValue();

            bool ContainsNestedAssignmentToIdentifier(SyntaxNode node) =>
                node.DescendantNodes().Any(x => IsAssignmentToIdentifier(x, identifierName, true, out _));
        }
    }
}
