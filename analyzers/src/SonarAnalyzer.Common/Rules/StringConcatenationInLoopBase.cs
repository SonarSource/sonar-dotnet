/*
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

        protected StringConcatenationInLoopBase() : base(DiagnosticId) { }
        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckSimpleAssignment, Language.SyntaxKind.SimpleAssignment);
            context.RegisterNodeAction(Language.GeneratedCodeRecognizer, CheckCompoundAssignment, CompoundAssignmentKinds);
        }

        private void CheckSimpleAssignment(SonarSyntaxNodeReportingContext context)
        {
            var assignment = (TAssignmentExpression)context.Node;

            if (Language.Syntax.AssignmentRight(assignment) is TBinaryExpression { } rightExpression
                && Language.Syntax.IsAnyKind(rightExpression, ExpressionConcatenationKinds)
                && Language.Syntax.AssignmentLeft(assignment) is var assigned
                && IsIdentifierOnTheRight(assigned, rightExpression)
                && IsSystemString(assigned, context.SemanticModel)
                && context.SemanticModel.GetSymbolInfo(assigned).Symbol is ILocalSymbol
                && AreNotDefinedInTheSameLoop(assigned, assignment, context.SemanticModel))
            {
                context.ReportIssue(SupportedDiagnostics[0], assignment);
            }
        }

        private void CheckCompoundAssignment(SonarSyntaxNodeReportingContext context)
        {
            var addAssignment = (TAssignmentExpression)context.Node;

            if (Language.Syntax.AssignmentLeft(addAssignment) is var expression
                && IsSystemString(expression, context.SemanticModel)
                && context.SemanticModel.GetSymbolInfo(expression).Symbol is ILocalSymbol
                && AreNotDefinedInTheSameLoop(expression, addAssignment, context.SemanticModel))
            {
                context.ReportIssue(SupportedDiagnostics[0], addAssignment);
            }
        }

        private bool IsIdentifierOnTheRight(SyntaxNode identifier, SyntaxNode expression)
        {
            while (expression is TBinaryExpression { } && Language.Syntax.IsAnyKind(expression, ExpressionConcatenationKinds))
            {
                var left = Language.Syntax.BinaryExpressionLeft(expression);
                var right = Language.Syntax.BinaryExpressionRight(expression);

                if (Language.Syntax.AreEquivalent(left, identifier) || Language.Syntax.AreEquivalent(right, identifier))
                {
                    return true;
                }
                // No need to recurse into the right branch as the only useful concatenation is flat `"a" + "b" + s`
                // `"a" + (s  + "b")` seems not worth to support.
                expression = left;
            }
            return false;
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

        private bool AreNotDefinedInTheSameLoop(SyntaxNode expression, SyntaxNode assignment, SemanticModel semanticModel) =>
            TryGetNearestLoop(assignment, out var nearestLoopForConcatenation)
            && !(semanticModel.GetSymbolInfo(expression).Symbol is { } symbol
                && symbol.GetFirstSyntaxRef() is { } declaration
                && TryGetNearestLoop(declaration, out var nearestLoop)
                && Language.Syntax.AreEquivalent(nearestLoop, nearestLoopForConcatenation));
    }
}
