﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers
{
    public abstract class ConstantValueFinder<TIdentifierNameSyntax, TVariableDeclaratorSyntax>
        where TIdentifierNameSyntax : SyntaxNode
        where TVariableDeclaratorSyntax : SyntaxNode
    {
        protected readonly SemanticModel SemanticModel;

        private readonly AssignmentFinder assignmentFinder;
        private readonly int nullLiteralExpressionSyntaxKind;

        protected abstract string IdentifierName(TIdentifierNameSyntax node);
        protected abstract SyntaxNode InitializerValue(TVariableDeclaratorSyntax node);
        protected abstract TVariableDeclaratorSyntax VariableDeclarator(SyntaxNode node);
        protected abstract bool IsPtrZero(SyntaxNode node);

        protected ConstantValueFinder(SemanticModel semanticModel, AssignmentFinder assignmentFinder, int nullLiteralExpressionSyntaxKind)
        {
            SemanticModel = semanticModel;
            this.assignmentFinder = assignmentFinder;
            this.nullLiteralExpressionSyntaxKind = nullLiteralExpressionSyntaxKind;
        }

        public object FindConstant(SyntaxNode node) =>
            FindConstant(node, null);

        private object FindConstant(SyntaxNode node, HashSet<SyntaxNode> visitedVariables)
        {
            if (node == null || node.RawKind == nullLiteralExpressionSyntaxKind) // Performance shortcut
            {
                return null;
            }

            if (IsPtrZero(node))
            {
                return 0;
            }

            return node.EnsureCorrectSemanticModelOrDefault(SemanticModel) is { } nodeSemanticModel
                ? nodeSemanticModel.GetConstantValue(node).Value ?? FindAssignedConstant(node, nodeSemanticModel, visitedVariables)
                : null;
        }

        private object FindAssignedConstant(SyntaxNode node, SemanticModel nodeSemanticModel, HashSet<SyntaxNode> visitedVariables)
        {
            return node is TIdentifierNameSyntax identifier
                ? FindConstant(assignmentFinder.FindLinearPrecedingAssignmentExpression(IdentifierName(identifier), node, FindFieldInitializer), visitedVariables)
                : null;

            SyntaxNode FindFieldInitializer()
            {
                if (nodeSemanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol fieldSymbol
                    && VariableDeclarator(fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()) is { } variable
                    && (visitedVariables == null || !visitedVariables.Contains(variable)))
                {
                    visitedVariables ??= new HashSet<SyntaxNode>();
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
}
