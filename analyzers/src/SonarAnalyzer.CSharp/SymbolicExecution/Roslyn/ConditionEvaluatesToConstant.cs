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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class ConditionEvaluatesToConstant : ConditionEvaluatesToConstantBase
{
    public static readonly DiagnosticDescriptor S2583 = DescriptorFactory.Create(DiagnosticId2583, MessageFormat);
    public static readonly DiagnosticDescriptor S2589 = DescriptorFactory.Create(DiagnosticId2589, MessageFormat);
    protected override DiagnosticDescriptor Rule => null;
    protected override DiagnosticDescriptor Rule2583 => S2583;
    protected override DiagnosticDescriptor Rule2589 => S2589;

    public override bool ShouldExecute()
    {
        var walker = new SyntaxKindWalker();
        walker.SafeVisit(Node);
        return walker.ContainsCondition;
    }

    private sealed class SyntaxKindWalker : SafeCSharpSyntaxWalker
    {
        public bool ContainsCondition { get; private set; }
        public override void Visit(SyntaxNode node)
        {
            if (!ContainsCondition)
            {
                ContainsCondition = node.IsAnyKind(
                                    SyntaxKind.CoalesceExpression,
                                    SyntaxKind.ConditionalAccessExpression,
                                    SyntaxKind.ConditionalExpression,
                                    SyntaxKind.DoStatement,
                                    SyntaxKind.ForStatement,
                                    SyntaxKind.IfStatement,
                                    SyntaxKind.LogicalAndExpression,
                                    SyntaxKind.LogicalOrExpression,
                                    SyntaxKindEx.SwitchExpression,
                                    SyntaxKind.SwitchStatement,
                                    SyntaxKind.WhileStatement);
                base.Visit(node);
            }
        }
    }

    protected override bool IsLeftCoalesceExpression(SyntaxNode syntax) =>
        syntax.Parent is BinaryExpressionSyntax { } binary
        && binary.OperatorToken.IsKind(SyntaxKind.QuestionQuestionToken)
        && binary.Left == syntax;

    protected override bool IsConditionalAccessExpression(SyntaxNode syntax) =>
        syntax.Parent is ConditionalAccessExpressionSyntax conditional && conditional.Expression == syntax;

    protected override bool IsForLoopIncrementor(SyntaxNode syntax) =>
        syntax.Parent is ForStatementSyntax forStatement && forStatement.Incrementors.Contains(syntax);

    protected override bool IsUsing(SyntaxNode syntax) =>
        (syntax.IsKind(SyntaxKind.VariableDeclaration) && syntax.Parent.IsKind(SyntaxKind.UsingStatement))
        || (syntax is LocalDeclarationStatementSyntax local && local.UsingKeyword().IsKind(SyntaxKind.UsingKeyword));

    protected override bool IsLockStatement(SyntaxNode syntax) =>
        syntax.IsKind(SyntaxKind.LockStatement);
}
