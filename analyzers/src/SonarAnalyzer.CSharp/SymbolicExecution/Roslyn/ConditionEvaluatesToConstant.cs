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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class ConditionEvaluatesToConstant : ConditionEvaluatesToConstantBase
{
    public static readonly DiagnosticDescriptor S2583 = DescriptorFactory.Create(DiagnosticId2583, MessageFormat);
    public static readonly DiagnosticDescriptor S2589 = DescriptorFactory.Create(DiagnosticId2589, MessageFormat);
    protected override DiagnosticDescriptor Rule => null;
    protected override DiagnosticDescriptor Rule2583 => S2583;
    protected override DiagnosticDescriptor Rule2589 => S2589;
    protected override string NullName => "null";

    public override bool ShouldExecute()
    {
        var walker = new SyntaxKindWalker();
        walker.SafeVisit(Node);
        return walker.ContainsCondition;
    }

    protected override bool IsInsideUsingDeclaration(SyntaxNode node) =>
        (node.IsKind(SyntaxKind.VariableDeclaration) && node.Parent.IsKind(SyntaxKind.UsingStatement))
        || (node is LocalDeclarationStatementSyntax local && local.UsingKeyword().IsKind(SyntaxKind.UsingKeyword));

    protected override bool IsLockStatement(SyntaxNode syntax) =>
        syntax.IsKind(SyntaxKind.LockStatement);

    internal sealed class SyntaxKindWalker : SafeCSharpSyntaxWalker
    {
        public bool ContainsCondition { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            ContainsCondition |= node.Kind() is SyntaxKindEx.CoalesceAssignmentExpression or SyntaxKindEx.SwitchExpression;
            if (!ContainsCondition)
            {
                base.Visit(node);
            }
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            ContainsCondition |= node.Kind() is SyntaxKind.CoalesceExpression or SyntaxKind.LogicalAndExpression or SyntaxKind.LogicalOrExpression;
            base.VisitBinaryExpression(node);
        }

        public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node) =>
            ContainsCondition = true;

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node) =>
            ContainsCondition = true;

        public override void VisitDoStatement(DoStatementSyntax node) =>
            ContainsCondition = true;

        public override void VisitForStatement(ForStatementSyntax node)
        {
            if (node.Condition == null)
            {
                base.VisitForStatement(node);
            }
            else
            {
                ContainsCondition = true;
            }
        }

        public override void VisitIfStatement(IfStatementSyntax node) =>
            ContainsCondition = true;

        public override void VisitSwitchStatement(SwitchStatementSyntax node) =>
            ContainsCondition = true;

        public override void VisitWhileStatement(WhileStatementSyntax node) =>
            ContainsCondition = true;
    }
}
