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

namespace SonarAnalyzer.Core.Trackers;

public abstract class AssignmentFinder
{
    /// <param name="anyAssignmentKind">'true' will find any AssignmentExpressionSyntax like =, +=, -=, &=. 'false' will find only '=' SimpleAssignmentExpression.</param>
    protected abstract bool IsAssignmentToIdentifier(SyntaxNode node, string identifierName, bool anyAssignmentKind, out SyntaxNode rightExpression);
    protected abstract bool IsIdentifierDeclaration(SyntaxNode node, string identifierName, out SyntaxNode initializer);
    protected abstract bool IsLoop(SyntaxNode node);
    protected abstract SyntaxNode GetTopMostContainingMethod(SyntaxNode node);

    public PrecedingAssignment FindLinearPrecedingAssignment(string identifierName, SyntaxNode current)
    {
        var method = GetTopMostContainingMethod(current);
        while (current != method && current?.Parent is not null)
        {
            if (IsLoop(current) && ContainsNestedAssignmentToIdentifier(current))
            {
                return PrecedingAssignment.Uncertain.Instance; // There's assignment inside this loop, value can be altered by each iteration
            }

            foreach (var statement in current.Parent.ChildNodes().TakeWhile(x => x != current).Reverse())
            {
                if (IsAssignmentToIdentifier(statement, identifierName, false, out var right))
                {
                    return new PrecedingAssignment.Found(right);
                }
                else if (IsIdentifierDeclaration(statement, identifierName, out var initializer))
                {
                    return new PrecedingAssignment.Found(initializer);
                }
                else if (ContainsNestedAssignmentToIdentifier(statement))
                {
                    return PrecedingAssignment.Uncertain.Instance; // Assignment inside nested statement (if, try, for, ...)
                }
            }
            current = current.Parent;
        }
        return PrecedingAssignment.None.Instance;

        bool ContainsNestedAssignmentToIdentifier(SyntaxNode node) =>
            node.DescendantNodes().Any(x => IsAssignmentToIdentifier(x, identifierName, true, out _));
    }
}

public abstract record PrecedingAssignment
{
    private PrecedingAssignment() { }

    public sealed record Found(SyntaxNode Assignment) : PrecedingAssignment;

    public sealed record None : PrecedingAssignment
    {
        public static readonly None Instance = new();

        private None() { }
    }

    public sealed record Uncertain : PrecedingAssignment
    {
        public static readonly Uncertain Instance = new();

        private Uncertain() { }
    }
}
