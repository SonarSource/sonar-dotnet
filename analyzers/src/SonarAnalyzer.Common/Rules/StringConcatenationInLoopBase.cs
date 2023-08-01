/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class StringConcatenationInLoopBase<TSyntaxKind, TAssignmentExpression, TBinaryExpression> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TAssignmentExpression : SyntaxNode
        where TBinaryExpression : SyntaxNode
    {
        protected const string DiagnosticId = "S1643";
        protected override string MessageFormat => "Use a StringBuilder instead.";

        protected abstract TSyntaxKind[] CompoundAssignmentKinds { get; }
        protected abstract ISet<TSyntaxKind> ExpressionConcatenationKinds { get; }
        protected abstract ISet<TSyntaxKind> LoopKinds { get; }
        protected abstract bool IsAddExpression(TBinaryExpression rightExpression);

        protected StringConcatenationInLoopBase() : base(DiagnosticId) { }
        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckSimpleAssignment, Language.SyntaxKind.SimpleAssignment);
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckCompoundAssignment, CompoundAssignmentKinds);
        }

        private void CheckSimpleAssignment(SonarSyntaxNodeReportingContext context)
        {
            var assignment = (TAssignmentExpression)context.Node;
            if (!IsSystemString(Language.Syntax.AssignmentLeft(assignment), context.SemanticModel))
            {
                return;
            }

            var rightExpression = Language.Syntax.AssignmentRight(assignment) as TBinaryExpression;
            if (!IsAddExpression(rightExpression))
            {
                return;
            }

            var assigned = Language.Syntax.AssignmentLeft(assignment);
            var leftOfConcatenation = GetInnerMostLeftOfConcatenation(rightExpression);
            if (leftOfConcatenation == null || !Language.Syntax.AreEquivalent(assigned, leftOfConcatenation))
            {
                return;
            }

            if (TryGetNearestLoop(assignment, out var nearestLoop)
                && !IsDefinedInLoop(assigned, nearestLoop, context.SemanticModel))
            {
                context.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], assignment.GetLocation()));
            }
        }

        private SyntaxNode GetInnerMostLeftOfConcatenation(TBinaryExpression binaryExpression)
        {
            var nestedLeft = Language.Syntax.BinaryExpressionLeft(binaryExpression);
            var nestedBinary = nestedLeft as TBinaryExpression;
            while (nestedBinary != null)
            {
                if (!Language.Syntax.IsAnyKind(nestedBinary, ExpressionConcatenationKinds))
                {
                    return null;
                }

                nestedLeft = Language.Syntax.BinaryExpressionLeft(nestedBinary);
                nestedBinary = nestedLeft as TBinaryExpression;
            }
            return nestedLeft;
        }

        private void CheckCompoundAssignment(SonarSyntaxNodeReportingContext context)
        {
            var addAssignment = (TAssignmentExpression)context.Node;

            if (IsSystemString(Language.Syntax.AssignmentLeft(addAssignment), context.SemanticModel)
                && (context.SemanticModel.GetSymbolInfo(Language.Syntax.AssignmentLeft(addAssignment)).Symbol is not ILocalSymbol
                    || (TryGetNearestLoop(addAssignment, out var nearestLoop) && !IsDefinedInLoop(Language.Syntax.AssignmentLeft(addAssignment), nearestLoop, context.SemanticModel))))
            {
                context.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], addAssignment.GetLocation()));
            }
        }

        private static bool IsSystemString(SyntaxNode node, SemanticModel semanticModel) =>
            node.IsKnownType(KnownType.System_String, semanticModel);

        private bool TryGetNearestLoop(SyntaxNode node, out SyntaxNode nearestLoop)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (Language.Syntax.IsAnyKind(parent, LoopKinds))
                {
                    nearestLoop = parent;
                    return true;
                }
                parent = parent.Parent;
            }
            nearestLoop = null;
            return false;
        }

        private bool IsDefinedInLoop(SyntaxNode expression, SyntaxNode nearestLoopForConcatenation, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(expression).Symbol is { } symbol
            && symbol.GetFirstSyntaxRef() is { } declaration
            && TryGetNearestLoop(declaration, out var nearestLoop)
            && nearestLoop == nearestLoopForConcatenation;

        /*
         private bool AreDefinedInTheSameLoop(SyntaxNode firstNode, SyntaxNode secondNode, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(firstNode).Symbol is { } firstSymbol
            && firstSymbol.GetFirstSyntaxRef() is { } firstRef
            && TryGetNearestLoop(firstRef, out var firstLoop)
            && TryGetNearestLoop(secondNode, out var secondLoop)
            && firstLoop == secondLoop;
         */
    }
}
