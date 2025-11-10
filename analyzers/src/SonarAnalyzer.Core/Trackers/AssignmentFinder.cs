/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Trackers;

public abstract class AssignmentFinder
{
    /// <param name="anyAssignmentKind">'true' will find any AssignmentExpressionSyntax like =, +=, -=, &=. 'false' will find only '=' SimpleAssignmentExpression.</param>
    protected abstract bool IsAssignmentToIdentifier(SyntaxNode node, string identifierName, bool anyAssignmentKind, out SyntaxNode rightExpression);
    protected abstract bool IsIdentifierDeclaration(SyntaxNode node, string identifierName, out SyntaxNode initializer);
    protected abstract bool IsLoop(SyntaxNode node);
    protected abstract SyntaxNode GetTopMostContainingMethod(SyntaxNode node);

    public SyntaxNode FindLinearPrecedingAssignmentExpression(string identifierName, SyntaxNode current, Func<SyntaxNode> defaultValue = null)
    {
        var method = GetTopMostContainingMethod(current);
        while (current != method && current?.Parent is not null)
        {
            if (IsLoop(current) && ContainsNestedAssignmentToIdentifier(current))
            {
                return null; // There's assignment inside this loop, value can be altered by each iteration
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
        return defaultValue?.Invoke();

        bool ContainsNestedAssignmentToIdentifier(SyntaxNode node) =>
            node.DescendantNodes().Any(x => IsAssignmentToIdentifier(x, identifierName, true, out _));
    }
}
