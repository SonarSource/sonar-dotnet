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

using SonarAnalyzer.Common.Walkers;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;

public class EmptyNullableValueAccess : EmptyNullableValueAccessBase
{
    private const string MessageFormat = "'{0}' is null on at least one execution path.";

    internal static readonly DiagnosticDescriptor S3655 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S3655;

    public override bool ShouldExecute()
    {
        var finder = new NullableAccessFinder();
        finder.SafeVisit(Node);
        return finder.HasPotentialNullableValueAccess;
    }

    private sealed class NullableAccessFinder : SafeCSharpSyntaxWalker
    {
        public bool HasPotentialNullableValueAccess { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            if (!HasPotentialNullableValueAccess)
            {
                base.Visit(node);
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.NameIs(nameof(Nullable<int>.Value)))
            {
                HasPotentialNullableValueAccess = true;
            }
            else
            {
                base.VisitMemberAccessExpression(node);
            }
        }

        public override void VisitCastExpression(CastExpressionSyntax node) =>
            HasPotentialNullableValueAccess = true;
    }
}
