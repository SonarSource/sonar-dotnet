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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Trackers;

public class VisualBasicAssignmentFinder : AssignmentFinder
{
    protected override SyntaxNode GetTopMostContainingMethod(SyntaxNode node) =>
        node.GetTopMostContainingMethod();

    /// <param name="anyAssignmentKind">'true' will find any AssignmentExpressionSyntax like =, +=, -=, &=. 'false' will find only '=' SimpleAssignmentExpression.</param>
    protected override bool IsAssignmentToIdentifier(SyntaxNode node, string identifierName, bool anyAssignmentKind, out SyntaxNode rightExpression)
    {
        if ((anyAssignmentKind || node.IsKind(SyntaxKind.SimpleAssignmentStatement))
            && node is AssignmentStatementSyntax assignment
            && assignment.Left.NameIs(identifierName))
        {
            rightExpression = assignment.Right;
            return true;
        }
        rightExpression = null;
        return false;
    }

    protected override bool IsIdentifierDeclaration(SyntaxNode node, string identifierName, out SyntaxNode initializer)
    {
        if (node is LocalDeclarationStatementSyntax declarationStatement
            && declarationStatement.Declarators.SingleOrDefault(MatchesIdentifierName) is { } declaration)
        {
            initializer = declaration.Initializer?.Value ?? (declaration.AsClause as AsNewClauseSyntax)?.NewExpression;
            return true;
        }
        initializer = null;
        return false;

        bool MatchesIdentifierName(VariableDeclaratorSyntax declarator) =>
            declarator.Names.Any(n => identifierName.Equals(n.Identifier.ValueText, StringComparison.OrdinalIgnoreCase));
    }

    protected override bool IsLoop(SyntaxNode node) =>
        node.IsAnyKind(SyntaxKind.ForBlock,
                       SyntaxKind.ForEachBlock,
                       SyntaxKind.WhileBlock,
                       SyntaxKind.DoLoopUntilBlock,
                       SyntaxKind.DoLoopWhileBlock,
                       SyntaxKind.DoUntilLoopBlock,
                       SyntaxKind.DoWhileLoopBlock);
}
