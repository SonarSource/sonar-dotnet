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

public sealed class CalculationsShouldNotOverflow : CalculationsShouldNotOverflowBase
{
    public static readonly DiagnosticDescriptor S3949 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3949;

    public override bool ShouldExecute()
    {
        if (ContainingSymbol?.Name == nameof(GetHashCode))
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
            if (!IsUnchecked && !HasOverflow)
            {
                IsUnchecked = node.IsAnyKind(SyntaxKind.UncheckedStatement, SyntaxKind.UncheckedExpression);
                HasOverflow = node.IsAnyKind(
                    SyntaxKind.AddExpression,
                    SyntaxKind.AddAssignmentExpression,
                    SyntaxKind.MultiplyExpression,
                    SyntaxKind.MultiplyAssignmentExpression,
                    SyntaxKind.SubtractExpression,
                    SyntaxKind.SubtractAssignmentExpression,
                    SyntaxKind.PostDecrementExpression,
                    SyntaxKind.PostIncrementExpression,
                    SyntaxKind.PreDecrementExpression,
                    SyntaxKind.PreIncrementExpression);
                base.Visit(node);
            }
        }
    }
}
