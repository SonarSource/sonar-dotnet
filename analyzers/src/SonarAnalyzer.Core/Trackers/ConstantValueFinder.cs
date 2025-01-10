/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

public abstract class ConstantValueFinder<TIdentifierNameSyntax, TVariableDeclaratorSyntax>
    where TIdentifierNameSyntax : SyntaxNode
    where TVariableDeclaratorSyntax : SyntaxNode
{
    protected readonly SemanticModel Model;

    private readonly AssignmentFinder assignmentFinder;
    private readonly int nullLiteralExpressionSyntaxKind;

    protected abstract string IdentifierName(TIdentifierNameSyntax node);
    protected abstract SyntaxNode InitializerValue(TVariableDeclaratorSyntax node);
    protected abstract TVariableDeclaratorSyntax VariableDeclarator(SyntaxNode node);
    protected abstract bool IsPtrZero(SyntaxNode node);

    protected ConstantValueFinder(SemanticModel model, AssignmentFinder assignmentFinder, int nullLiteralExpressionSyntaxKind)
    {
        Model = model;
        this.assignmentFinder = assignmentFinder;
        this.nullLiteralExpressionSyntaxKind = nullLiteralExpressionSyntaxKind;
    }

    public object FindConstant(SyntaxNode node) =>
        FindConstant(node, null);

    private object FindConstant(SyntaxNode node, HashSet<SyntaxNode> visitedVariables)
    {
        if (node is null || node.RawKind == nullLiteralExpressionSyntaxKind) // Performance shortcut
        {
            return null;
        }

        if (IsPtrZero(node))
        {
            return 0;
        }

        return node.EnsureCorrectSemanticModelOrDefault(Model) is { } nodeModel
            ? nodeModel.GetConstantValue(node).Value ?? FindAssignedConstant(node, nodeModel, visitedVariables)
            : null;
    }

    private object FindAssignedConstant(SyntaxNode node, SemanticModel model, HashSet<SyntaxNode> visitedVariables)
    {
        return node is TIdentifierNameSyntax identifier
            ? FindConstant(assignmentFinder.FindLinearPrecedingAssignmentExpression(IdentifierName(identifier), node, FindFieldInitializer), visitedVariables)
            : null;

        SyntaxNode FindFieldInitializer()
        {
            if (model.GetSymbolInfo(identifier).Symbol is IFieldSymbol fieldSymbol
                && VariableDeclarator(fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()) is { } variable
                && (visitedVariables is null || !visitedVariables.Contains(variable)))
            {
                visitedVariables ??= new();
                visitedVariables.Add(variable);
                return InitializerValue(variable);
            }
            else
            {
                return null;
            }
        }
    }
}
