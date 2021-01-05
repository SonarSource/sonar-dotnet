/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public abstract class ConstantValueFinder<TIdentifierNameSyntax, TVariableDeclaratorSyntax>
        where TIdentifierNameSyntax : SyntaxNode
        where TVariableDeclaratorSyntax : SyntaxNode
    {
        private readonly SemanticModel semanticModel;
        private readonly AssignmentFinder assignmentFinder;
        private readonly int nullLiteralExpressionSyntaxKind;

        protected abstract string IdentifierName(TIdentifierNameSyntax node);
        protected abstract SyntaxNode InitializerValue(TVariableDeclaratorSyntax node);
        protected abstract TVariableDeclaratorSyntax VariableDeclarator(SyntaxNode node);

        protected ConstantValueFinder(SemanticModel semanticModel, AssignmentFinder assignmentFinder, int nullLiteralExpressionSyntaxKind)
        {
            this.semanticModel = semanticModel;
            this.assignmentFinder = assignmentFinder;
            this.nullLiteralExpressionSyntaxKind = nullLiteralExpressionSyntaxKind;
        }

        public object FindConstant(SyntaxNode node) =>
            FindConstant(node, null);

        private object FindConstant(SyntaxNode node, HashSet<SyntaxNode> visitedVariables) =>
            node == null || node.RawKind == nullLiteralExpressionSyntaxKind  // Performance shortcut
            ? null
            : semanticModel.GetConstantValue(node).Value ?? FindAssignedConstant(node, visitedVariables);

        private object FindAssignedConstant(SyntaxNode node, HashSet<SyntaxNode> visitedVariables)
        {
            return node is TIdentifierNameSyntax identifier
                ? FindConstant(assignmentFinder.FindLinearPrecedingAssignmentExpression(IdentifierName(identifier), node, FindFieldInitializer), visitedVariables)
                : null;

            SyntaxNode FindFieldInitializer()
            {
                if (semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol fieldSymbol
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
