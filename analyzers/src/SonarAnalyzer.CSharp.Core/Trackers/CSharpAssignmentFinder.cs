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

namespace SonarAnalyzer.CSharp.Core.Trackers;

public class CSharpAssignmentFinder : AssignmentFinder
{
    protected override SyntaxNode GetTopMostContainingMethod(SyntaxNode node) =>
        node.GetTopMostContainingMethod();

    /// <param name="anyAssignmentKind">'true' will find any AssignmentExpressionSyntax like =, +=, -=, &=. 'false' will find only '=' SimpleAssignmentExpression.</param>
    protected override bool IsAssignmentToIdentifier(SyntaxNode node, string identifierName, bool anyAssignmentKind, out SyntaxNode rightExpression)
    {
        if (node.IsKind(SyntaxKindEx.GlobalStatement))
        {
            node = ((GlobalStatementSyntax)node).Statement;
        }

        if (node is ExpressionStatementSyntax statement)
        {
            node = statement.Expression;
        }
        if ((anyAssignmentKind || node.IsKind(SyntaxKind.SimpleAssignmentExpression))
            && IdentifierMutation(node, identifierName) is { } mutation)
        {
            rightExpression = mutation;
            return true;
        }
        rightExpression = null;
        return false;
    }

    protected override bool IsIdentifierDeclaration(SyntaxNode node, string identifierName, out SyntaxNode initializer)
    {
        if (node.IsKind(SyntaxKindEx.GlobalStatement))
        {
            node = ((GlobalStatementSyntax)node).Statement;
        }

        if (node is LocalDeclarationStatementSyntax declarationStatement
            && declarationStatement.Declaration.Variables.SingleOrDefault(x => x.Identifier.ValueText == identifierName) is { } declaration)
        {
            initializer = declaration.Initializer?.Value;
            return true;
        }

        initializer = null;
        return false;
    }

    protected override bool IsLoop(SyntaxNode node) =>
        node?.Kind() is SyntaxKind.ForStatement or SyntaxKind.ForEachStatement or SyntaxKind.WhileStatement or SyntaxKind.DoStatement;

    /// <summary>
    /// If <paramref name="identifierName"/> is mutated inside <paramref name="mutation"/> then
    /// the expression representing the new value is returned. The returned expression might be
    /// the reference to the identifier itself, e.g. in a case like <c>i++;</c>.
    /// </summary>
    private static SyntaxNode IdentifierMutation(SyntaxNode mutation, string identifierName) =>
        mutation switch
        {
            AssignmentExpressionSyntax assignment
                when assignment.MapAssignmentArguments().FirstOrDefault(x => x.Left.NameIs(identifierName)) is { Right: { } right } => right,
            PostfixUnaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.PostIncrementExpression or (int)SyntaxKind.PostDecrementExpression,
                Operand: { } operand,
            } when operand.NameIs(identifierName) => operand,
            PrefixUnaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.PreIncrementExpression or (int)SyntaxKind.PreDecrementExpression or (int)SyntaxKind.AddressOfExpression,
                Operand: { } operand,
            } when operand.NameIs(identifierName) => operand,
            // Passing by ref or out is likely mutating the argument so we assume it is assigned a value in the called method.
            ArgumentSyntax
            {
                RefOrOutKeyword.RawKind: (int)SyntaxKind.RefKeyword or (int)SyntaxKind.OutKeyword,
                Expression: { } argumentExpression
            } when argumentExpression.NameIs(identifierName) => argumentExpression,
            _ => null,
        };
}
