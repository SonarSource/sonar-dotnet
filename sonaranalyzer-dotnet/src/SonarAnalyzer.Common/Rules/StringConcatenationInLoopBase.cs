/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class StringConcatenationInLoopBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S1643";
        protected const string MessageFormat = "Use a StringBuilder instead.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class StringConcatenationInLoopBase<TLanguageKindEnum, TAssignmentExpression, TBinaryExpression>
            : StringConcatenationInLoopBase
        where TLanguageKindEnum : struct
        where TAssignmentExpression : SyntaxNode
        where TBinaryExpression : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                CheckCompoundAssignment,
                CompoundAssignmentKinds.ToArray());

            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                CheckSimpleAssignment,
                SimpleAssignmentKinds.ToArray());
        }

        private void CheckSimpleAssignment(SyntaxNodeAnalysisContext context)
        {
            var assignment = (TAssignmentExpression)context.Node;
            if (!IsString(GetLeft(assignment), context.SemanticModel))
            {
                return;
            }

            var rightExpression = GetRight(assignment) as TBinaryExpression;
            if (!IsAddExpression(rightExpression))
            {
                return;
            }

            var assigned = GetLeft(assignment);
            var leftOfConcatenation = GetInnerMostLeftOfConcatenation(rightExpression);
            if (leftOfConcatenation == null ||
                !AreEquivalent(assigned, leftOfConcatenation))
            {
                return;
            }

            if (!TryGetNearestLoop(assignment, out var nearestLoop) ||
                IsDefinedInLoop(assigned, nearestLoop, context.SemanticModel))
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], assignment.GetLocation()));
        }

        protected abstract bool IsAddExpression(TBinaryExpression rightExpression);

        private SyntaxNode GetInnerMostLeftOfConcatenation(TBinaryExpression binaryExpression)
        {
            var nestedLeft = GetLeft(binaryExpression);
            var nestedBinary = nestedLeft as TBinaryExpression;
            while (nestedBinary != null)
            {
                if (!IsExpressionConcatenation(nestedBinary))
                {
                    return null;
                }

                nestedLeft = GetLeft(nestedBinary);
                nestedBinary = nestedLeft as TBinaryExpression;
            }
            return nestedLeft;
        }

        private void CheckCompoundAssignment(SyntaxNodeAnalysisContext context)
        {
            var addAssignment = (TAssignmentExpression)context.Node;
            if (!IsString(GetLeft(addAssignment), context.SemanticModel))
            {
                return;
            }

            if (!TryGetNearestLoop(addAssignment, out var nearestLoop))
            {
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(GetLeft(addAssignment)).Symbol is ILocalSymbol symbol &&
                IsDefinedInLoop(GetLeft(addAssignment), nearestLoop, context.SemanticModel))
            {
                return;
            }

            context.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], addAssignment.GetLocation()));
        }

        protected abstract bool IsExpressionConcatenation(TBinaryExpression addExpression);

        protected abstract SyntaxNode GetLeft(TAssignmentExpression assignment);

        protected abstract SyntaxNode GetRight(TAssignmentExpression assignment);

        protected abstract SyntaxNode GetLeft(TBinaryExpression binary);

        protected abstract bool AreEquivalent(SyntaxNode node1, SyntaxNode node2);

        protected abstract ImmutableArray<TLanguageKindEnum> SimpleAssignmentKinds { get; }
        protected abstract ImmutableArray<TLanguageKindEnum> CompoundAssignmentKinds { get; }

        private static bool IsString(SyntaxNode node, SemanticModel semanticModel)
        {
            return semanticModel.GetTypeInfo(node).Type
                .Is(KnownType.System_String);
        }

        private bool TryGetNearestLoop(SyntaxNode node, out SyntaxNode nearestLoop)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (IsInLoop(parent))
                {
                    nearestLoop = parent;
                    return true;
                }
                parent = parent.Parent;
            }
            nearestLoop = null;
            return false;
        }

        protected abstract bool IsInLoop(SyntaxNode node);

        private bool IsDefinedInLoop(SyntaxNode expression, SyntaxNode nearestLoopForConcatenation,
                SemanticModel semanticModel)
        {
            var symbol = semanticModel.GetSymbolInfo(expression).Symbol as ILocalSymbol;

            var declaration = symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            if (declaration == null)
            {
                return false;
            }

            if (!TryGetNearestLoop(declaration, out var nearestLoop))
            {
                return false;
            }

            return nearestLoop == nearestLoopForConcatenation;
        }
    }
}
