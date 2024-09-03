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

public sealed class CalculationsShouldNotOverflow : CalculationsShouldNotOverflowBase
{
    public static readonly DiagnosticDescriptor S3949 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3949;

    public override bool ShouldExecute()
    {
        if (ContainingSymbol is IMethodSymbol method && method.IsObjectGetHashCode())
        {
            return false;
        }
        else
        {
            var walker = new SyntaxKindWalker();
            walker.SafeVisit(Node);
            return walker.HasOverflow && !walker.IsUnchecked;
        }
    }

    internal sealed class SyntaxKindWalker : SafeCSharpSyntaxWalker
    {
        public bool IsUnchecked { get; private set; }
        public bool HasOverflow { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            if (IsUnchecked)
            {
                return; // We have an unchecked context: stop visiting
            }
            base.Visit(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            HasOverflow |= node.Kind() is SyntaxKind.AddExpression or SyntaxKind.MultiplyExpression or SyntaxKind.SubtractExpression;
            base.VisitBinaryExpression(node);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            HasOverflow |= node.Kind() is SyntaxKind.AddAssignmentExpression or SyntaxKind.MultiplyAssignmentExpression or SyntaxKind.SubtractAssignmentExpression;
            base.VisitAssignmentExpression(node);
        }

        public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax node)
        {
            HasOverflow |= node.Kind() is SyntaxKind.PostDecrementExpression or SyntaxKind.PostIncrementExpression;
            base.VisitPostfixUnaryExpression(node);
        }

        public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            HasOverflow |= node.Kind() is SyntaxKind.PreDecrementExpression or SyntaxKind.PreIncrementExpression;
            base.VisitPrefixUnaryExpression(node);
        }

        public override void VisitCheckedExpression(CheckedExpressionSyntax node)
        {
            IsUnchecked |= node.Kind() is SyntaxKind.UncheckedExpression;
            base.VisitCheckedExpression(node);
        }

        public override void VisitCheckedStatement(CheckedStatementSyntax node)
        {
            IsUnchecked |= node.Kind() is SyntaxKind.UncheckedStatement;
            base.VisitCheckedStatement(node);
        }
    }
}
